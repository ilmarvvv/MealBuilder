using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class AddIngredientModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public AddIngredientModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public Recipe Recipe { get; set; } = new();

        public SelectList Ingredients { get; set; } = null!;

        [BindProperty]
        public RecipeIngredient RecipeIngredient { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes
                .FirstOrDefaultAsync(recipe =>
                    recipe.Id == id &&
                    recipe.OwnerId == _currentUser.UserId);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;
            RecipeIngredient.RecipeId = recipe.Id;

            await LoadIngredientsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("RecipeIngredient.Recipe");
            ModelState.Remove("RecipeIngredient.Ingredient");

            bool ingredientExists = await _context.Ingredients
                .AnyAsync(ingredient =>
                    ingredient.Id == RecipeIngredient.IngredientId &&
                    ingredient.OwnerId == _currentUser.UserId);

            if (!ingredientExists)
            {
                ModelState.AddModelError(
                    "RecipeIngredient.IngredientId",
                    "Ingredient was not found.");
            }

            bool recipeExists = await _context.Recipes
                .AnyAsync(recipe =>
                    recipe.Id == RecipeIngredient.RecipeId &&
                    recipe.OwnerId == _currentUser.UserId);

            if (!recipeExists)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Recipe? recipe = await _context.Recipes
                    .FirstOrDefaultAsync(recipe =>
                        recipe.Id == RecipeIngredient.RecipeId &&
                        recipe.OwnerId == _currentUser.UserId);

                if (recipe is not null)
                {
                    Recipe = recipe;
                }

                await LoadIngredientsAsync();
                return Page();
            }

            int ingredientCount = await _context.RecipeIngredients
                .CountAsync(recipeIngredient => recipeIngredient.RecipeId == RecipeIngredient.RecipeId);

            int componentCount = await _context.RecipeComponents
                .CountAsync(recipeComponent => recipeComponent.ParentRecipeId == RecipeIngredient.RecipeId);

            RecipeIngredient.Position = ingredientCount + componentCount + 1;

            _context.RecipeIngredients.Add(RecipeIngredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = RecipeIngredient.RecipeId });
        }

        private async Task LoadIngredientsAsync()
        {
            List<Ingredient> ingredients = await _context.Ingredients
                .Where(ingredient => ingredient.OwnerId == _currentUser.UserId)
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            Ingredients = new SelectList(ingredients, "Id", "Name");
        }
    }
}
