using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly DailyPlanCalculationService _dailyPlanCalculationService;

        public DetailsModel(
            AppDbContext context,
            DailyPlanCalculationService dailyPlanCalculationService)
        {
            _context = context;
            _dailyPlanCalculationService = dailyPlanCalculationService;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        public RecipeNutritionTotals Totals { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.Ingredient)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Items)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.Recipe)
                .ThenInclude(recipe => recipe!.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.Recipe)
                .ThenInclude(recipe => recipe!.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(dailyPlan => dailyPlan.Id == id);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            DailyPlan = dailyPlan;
            Totals = _dailyPlanCalculationService.Calculate(dailyPlan);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int dailyPlanItemId)
        {
            DailyPlanItem? dailyPlanItem = await _context.DailyPlanItems
                .FindAsync(dailyPlanItemId);

            if (dailyPlanItem is null)
            {
                return NotFound();
            }

            int dailyPlanId = dailyPlanItem.DailyPlanId;

            _context.DailyPlanItems.Remove(dailyPlanItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = dailyPlanId });
        }
    }
}
