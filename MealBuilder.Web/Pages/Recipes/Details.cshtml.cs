using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
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
        private readonly CurrentUserAccessor _currentUser;

        public DetailsModel(
            AppDbContext context,
            RecipeCalculationService recipeCalculationService,
            CurrentUserAccessor currentUser)
        {
            _context = context;
            _recipeCalculationService = recipeCalculationService;
            _currentUser = currentUser;
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
                .FirstOrDefaultAsync(recipe => recipe.Id == id && recipe.OwnerId == _currentUser.UserId);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;
            Totals = _recipeCalculationService.Calculate(recipe);
            PerDayTotals = _recipeCalculationService.Divide(Totals, recipe.DefaultPlannedDays);
            PerServingTotals = _recipeCalculationService.Divide(Totals, recipe.TotalServings);
            EstimatedWeightGrams = _recipeCalculationService.CalculateEstimatedWeight(recipe);
            EffectiveWeightGrams = _recipeCalculationService.CalculateEffectiveWeight(recipe);
            RecipeContentSummaries = recipe.RecipeIngredients
                .Select(recipeIngredient => new RecipeContentSummary
                {
                    Id = recipeIngredient.Id,
                    Position = recipeIngredient.Position,
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
                        Position = recipeComponent.Position,
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
                .OrderBy(recipeContent => recipeContent.Position)
                .ToList();

            return Page();
        }
    }
}