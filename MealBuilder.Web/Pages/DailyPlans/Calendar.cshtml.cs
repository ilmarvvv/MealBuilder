using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class CalendarModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly DailyPlanCalculationService _dailyPlanCalculationService;

        public CalendarModel(
            AppDbContext context,
            DailyPlanCalculationService dailyPlanCalculationService)
        {
            _context = context;
            _dailyPlanCalculationService = dailyPlanCalculationService;
        }

        public List<DailyPlanCalendarDay> Days { get; set; } = [];

        public DateOnly WeekStart { get; set; }

        public RecipeNutritionTotals DailyAverageTotals { get; set; } = new();

        public DateOnly PreviousWeekStart => WeekStart.AddDays(-7);

        public DateOnly NextWeekStart => WeekStart.AddDays(7);
        public string PreviousWeekStartRouteValue =>
    PreviousWeekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public string NextWeekStartRouteValue =>
            NextWeekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public async Task OnGetAsync(DateOnly? startDate)
        {
            DateOnly weekStart = GetWeekStart(startDate ?? DateOnly.FromDateTime(DateTime.Today));
            DateOnly weekEnd = weekStart.AddDays(6);

            WeekStart = weekStart;

            List<DailyPlan> dailyPlans = await _context.DailyPlans
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.Ingredient)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .ThenInclude(recipe => recipe!.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Items)
                .Include(dailyPlan => dailyPlan.DailyPlanItems)
                .ThenInclude(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .ThenInclude(recipe => recipe!.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
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
                .Where(dailyPlan => dailyPlan.Date >= weekStart && dailyPlan.Date <= weekEnd)
                .OrderBy(dailyPlan => dailyPlan.Date)
                .ToListAsync();

            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                DateOnly date = weekStart.AddDays(dayOffset);
                DailyPlan? dailyPlan = dailyPlans.FirstOrDefault(dailyPlan => dailyPlan.Date == date);

                Days.Add(new DailyPlanCalendarDay
                {
                    Date = date,
                    DailyPlan = dailyPlan,
                    Totals = dailyPlan is null
                        ? new RecipeNutritionTotals()
                        : _dailyPlanCalculationService.Calculate(dailyPlan)
                });
            }

            DailyAverageTotals = new RecipeNutritionTotals
            {
                Calories = Days.Sum(day => day.Totals.Calories) / 7,
                Protein = Days.Sum(day => day.Totals.Protein) / 7,
                Fiber = Days.Sum(day => day.Totals.Fiber) / 7,
                Sugar = Days.Sum(day => day.Totals.Sugar) / 7,
                Salt = Days.Sum(day => day.Totals.Salt) / 7
            };
        }

        private static DateOnly GetWeekStart(DateOnly date)
        {
            int diff = ((int)date.DayOfWeek + 6) % 7;

            return date.AddDays(-diff);
        }
    }

    public class DailyPlanCalendarDay
    {
        public DateOnly Date { get; set; }

        public DailyPlan? DailyPlan { get; set; }

        public RecipeNutritionTotals Totals { get; set; } = new();
    }
}
