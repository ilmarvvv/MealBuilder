using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MealBuilder.Web.Pages.Menus
{
    public class CalendarModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly MenuCalculationService _menuCalculationService;

        public CalendarModel(
            AppDbContext context,
            MenuCalculationService menuCalculationService)
        {
            _context = context;
            _menuCalculationService = menuCalculationService;
        }

        public List<MenuCalendarDay> Days { get; set; } = [];

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

            List<Menu> menus = await _context.Menus
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .ThenInclude(recipe => recipe!.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .ThenInclude(recipe => recipe!.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Recipe)
                .ThenInclude(recipe => recipe!.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Recipe)
                .ThenInclude(recipe => recipe!.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Where(menu => menu.Date >= weekStart && menu.Date <= weekEnd)
                .OrderBy(menu => menu.Date)
                .ToListAsync();

            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                DateOnly date = weekStart.AddDays(dayOffset);
                Menu? menu = menus.FirstOrDefault(menu => menu.Date == date);

                Days.Add(new MenuCalendarDay
                {
                    Date = date,
                    Menu = menu,
                    Totals = menu is null
                        ? new RecipeNutritionTotals()
                        : _menuCalculationService.Calculate(menu)
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

    public class MenuCalendarDay
    {
        public DateOnly Date { get; set; }

        public Menu? Menu { get; set; }

        public RecipeNutritionTotals Totals { get; set; } = new();
    }
}
