using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
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

        private readonly CurrentUserAccessor _currentUser;

        public DetailsModel(
            AppDbContext context,
            DailyPlanCalculationService dailyPlanCalculationService,
            CurrentUserAccessor currentUser)
        {
            _context = context;
            _dailyPlanCalculationService = dailyPlanCalculationService;
            _currentUser = currentUser;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        public RecipeNutritionTotals Totals { get; set; } = new();

        public bool IsPersisted => DailyPlan.Id > 0;

        public async Task<IActionResult> OnGetAsync(int? id, DateOnly? date)
        {
            IQueryable<DailyPlan> query = _context.DailyPlans
                .Where(dailyPlan => dailyPlan.OwnerId == _currentUser.UserId)
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
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient);

            DailyPlan? dailyPlan;

            if (id.HasValue)
            {
                dailyPlan = await query
                    .FirstOrDefaultAsync(dailyPlan => dailyPlan.Id == id.Value);

                if (dailyPlan is null)
                {
                    return NotFound();
                }
            }
            else
            {
                if (!date.HasValue)
                {
                    return BadRequest();
                }

                dailyPlan = await query
                    .FirstOrDefaultAsync(dailyPlan => dailyPlan.Date == date.Value);

                if (dailyPlan is null)
                {
                    DailyPlan = new DailyPlan
                    {
                        Date = date.Value,
                        Name = $"Daily Plan {date.Value}"
                    };

                    return Page();
                }
            }

            DailyPlan = dailyPlan;
            Totals = _dailyPlanCalculationService.Calculate(dailyPlan);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int dailyPlanItemId)
        {
            DailyPlanItem? dailyPlanItem = await _context.DailyPlanItems
                .FirstOrDefaultAsync(dailyPlanItem =>
                    dailyPlanItem.Id == dailyPlanItemId &&
                    dailyPlanItem.DailyPlan.OwnerId == _currentUser.UserId);

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
