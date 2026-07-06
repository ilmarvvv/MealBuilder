using MealBuilder.Web.Data;
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

        public AddIngredientModel(AppDbContext context)
        {
            _context = context;
        }

        public Recipe Recipe { get; set; } = new();

        public SelectList Ingredients { get; set; } = null!;

        [BindProperty]
        public RecipeIngredient RecipeIngredient { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

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

            if (!ModelState.IsValid)
            {
                Recipe? recipe = await _context.Recipes.FindAsync(RecipeIngredient.RecipeId);

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
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            Ingredients = new SelectList(ingredients, "Id", "Name");
        }
    }
}
