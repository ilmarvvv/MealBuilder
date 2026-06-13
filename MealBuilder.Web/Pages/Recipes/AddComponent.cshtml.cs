using MealBuilder.Web.Data;
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

        public AddComponentModel(AppDbContext context)
        {
            _context = context;
        }

        public Recipe Recipe { get; set; } = new();

        public SelectList ComponentRecipes { get; set; } = null!;

        [BindProperty]
        public RecipeComponent RecipeComponent { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

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
                Recipe? recipe = await _context.Recipes.FindAsync(RecipeComponent.ParentRecipeId);

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

            return RedirectToPage("./Details", new { id = RecipeComponent.ParentRecipeId });
        }

        private async Task LoadComponentRecipesAsync(int parentRecipeId)
        {
            List<int> alreadyAddedComponentRecipeIds = await _context.RecipeComponents
                .Where(recipeComponent => recipeComponent.ParentRecipeId == parentRecipeId)
                .Select(recipeComponent => recipeComponent.ComponentRecipeId)
                .ToListAsync();

            List<Recipe> recipes = await _context.Recipes
                .Where(recipe => recipe.Id != parentRecipeId)
                .Where(recipe => !alreadyAddedComponentRecipeIds.Contains(recipe.Id))
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();

            ComponentRecipes = new SelectList(recipes, "Id", "Name");
        }
    }
}
