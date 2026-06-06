using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly RecipeCalculationService _recipeCalculationService;

        public DetailsModel(AppDbContext context, RecipeCalculationService recipeCalculationService)
        {
            _context = context;
            _recipeCalculationService = recipeCalculationService;
        }

        public Recipe Recipe { get; set; } = new();

        public RecipeNutritionTotals Totals { get; set; } = new();

        public RecipeNutritionTotals PerServingTotals { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(recipe => recipe.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipe => recipe.Id == id);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;
            Totals = _recipeCalculationService.Calculate(recipe);
            PerServingTotals = _recipeCalculationService.Divide(Totals, recipe.Servings);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveIngredientAsync(int recipeIngredientId)
        {
            RecipeIngredient? recipeIngredient = await _context.RecipeIngredients
                .FindAsync(recipeIngredientId);

            if (recipeIngredient is null)
            {
                return NotFound();
            }

            int recipeId = recipeIngredient.RecipeId;

            _context.RecipeIngredients.Remove(recipeIngredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = recipeId });
        }

        public async Task<IActionResult> OnPostRemoveComponentAsync(int recipeComponentId)
        {
            RecipeComponent? recipeComponent = await _context.RecipeComponents
                .FindAsync(recipeComponentId);

            if (recipeComponent is null)
            {
                return NotFound();
            }

            int recipeId = recipeComponent.ParentRecipeId;

            _context.RecipeComponents.Remove(recipeComponent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = recipeId });
        }
    }
}
