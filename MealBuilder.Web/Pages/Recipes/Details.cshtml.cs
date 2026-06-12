using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using MealBuilder.Web.ViewModels;
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

        public RecipeNutritionTotals PerDayTotals { get; set; } = new();

        public RecipeNutritionTotals PerServingTotals { get; set; } = new();

        public decimal EstimatedWeightGrams { get; set; }

        public decimal EffectiveWeightGrams { get; set; }

        public List<RecipeContentSummary> RecipeContentSummaries { get; set; } = [];

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
            PerDayTotals = _recipeCalculationService.Divide(Totals, recipe.DefaultPlannedDays);
            PerServingTotals = _recipeCalculationService.Divide(Totals, recipe.Servings);
            EstimatedWeightGrams = _recipeCalculationService.CalculateEstimatedWeight(recipe);
            EffectiveWeightGrams = _recipeCalculationService.CalculateEffectiveWeight(recipe);
            RecipeContentSummaries = recipe.RecipeIngredients
    .Select(recipeIngredient => new RecipeContentSummary
    {
        Id = recipeIngredient.Id,
        Type = "Ingredient",
        Name = recipeIngredient.Ingredient.Name,
        Grams = recipeIngredient.Grams,
        Calories = recipeIngredient.Ingredient.CaloriesPer100g * recipeIngredient.Grams / 100,
        Protein = recipeIngredient.Ingredient.ProteinPer100g * recipeIngredient.Grams / 100,
        Fiber = recipeIngredient.Ingredient.FiberPer100g * recipeIngredient.Grams / 100,
        Sugar = recipeIngredient.Ingredient.SugarPer100g * recipeIngredient.Grams / 100,
        Salt = recipeIngredient.Ingredient.SaltPer100g * recipeIngredient.Grams / 100
    })
    .Concat(recipe.Components.Select(recipeComponent =>
    {
        decimal componentTotalWeight = _recipeCalculationService.CalculateEffectiveWeight(recipeComponent.ComponentRecipe);
        RecipeNutritionTotals componentTotals = _recipeCalculationService.Calculate(recipeComponent.ComponentRecipe);
        decimal ratio = componentTotalWeight > 0 ? recipeComponent.Grams / componentTotalWeight : 0;

        return new RecipeContentSummary
        {
            Id = recipeComponent.Id,
            Type = "Recipe",
            Name = recipeComponent.ComponentRecipe.Name,
            Grams = recipeComponent.Grams,
            Calories = componentTotals.Calories * ratio,
            Protein = componentTotals.Protein * ratio,
            Fiber = componentTotals.Fiber * ratio,
            Sugar = componentTotals.Sugar * ratio,
            Salt = componentTotals.Salt * ratio
        };
    }))
    .ToList();
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
