using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class AddComponentModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public AddComponentModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public Recipe Recipe { get; set; } = new();

        public SelectList ComponentRecipes { get; set; } = null!;

        [BindProperty]
        public RecipeComponent RecipeComponent { get; set; } = new();

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
            RecipeComponent.ParentRecipeId = recipe.Id;

            await LoadComponentRecipesAsync(recipe.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("RecipeComponent.ParentRecipe");
            ModelState.Remove("RecipeComponent.ComponentRecipe");

            if (RecipeComponent.ParentRecipeId == RecipeComponent.ComponentRecipeId)
            {
                ModelState.AddModelError("RecipeComponent.ComponentRecipeId", "A recipe cannot contain itself.");
            }

            bool parentRecipeExists = await _context.Recipes
                .AnyAsync(recipe =>
                    recipe.Id == RecipeComponent.ParentRecipeId &&
                    recipe.OwnerId == _currentUser.UserId);

            if (!parentRecipeExists)
            {
                return NotFound();
            }

            bool componentRecipeExists = await _context.Recipes
                .AnyAsync(recipe =>
                    recipe.Id == RecipeComponent.ComponentRecipeId &&
                    recipe.OwnerId == _currentUser.UserId);

            if (!componentRecipeExists)
            {
                ModelState.AddModelError(
                    "RecipeComponent.ComponentRecipeId",
                    "Recipe component was not found.");
            }

            bool duplicateExists = await _context.RecipeComponents
                .AnyAsync(recipeComponent =>
                    recipeComponent.ParentRecipeId == RecipeComponent.ParentRecipeId &&
                    recipeComponent.ComponentRecipeId == RecipeComponent.ComponentRecipeId);

            if (duplicateExists)
            {
                ModelState.AddModelError("RecipeComponent.ComponentRecipeId", "This recipe component has already been added.");
            }

            if (!ModelState.IsValid)
            {
                Recipe? recipe = await _context.Recipes
                    .FirstOrDefaultAsync(recipe =>
                        recipe.Id == RecipeComponent.ParentRecipeId &&
                        recipe.OwnerId == _currentUser.UserId);

                if (recipe is not null)
                {
                    Recipe = recipe;
                }

                await LoadComponentRecipesAsync(RecipeComponent.ParentRecipeId);
                return Page();
            }

            int ingredientCount = await _context.RecipeIngredients
                .CountAsync(recipeIngredient => recipeIngredient.RecipeId == RecipeComponent.ParentRecipeId);

            int componentCount = await _context.RecipeComponents
                .CountAsync(recipeComponent => recipeComponent.ParentRecipeId == RecipeComponent.ParentRecipeId);

            RecipeComponent.Position = ingredientCount + componentCount + 1;

            _context.RecipeComponents.Add(RecipeComponent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = RecipeComponent.ParentRecipeId });
        }

        private async Task LoadComponentRecipesAsync(int parentRecipeId)
        {
            List<int> alreadyAddedComponentRecipeIds = await _context.RecipeComponents
                .Where(recipeComponent => recipeComponent.ParentRecipeId == parentRecipeId)
                .Select(recipeComponent => recipeComponent.ComponentRecipeId)
                .ToListAsync();

            List<Recipe> recipes = await _context.Recipes
                .Where(recipe =>
                    recipe.Id != parentRecipeId &&
                    recipe.OwnerId == _currentUser.UserId)
                .Where(recipe => !alreadyAddedComponentRecipeIds.Contains(recipe.Id))
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();

            ComponentRecipes = new SelectList(recipes, "Id", "Name");
        }
    }
}
