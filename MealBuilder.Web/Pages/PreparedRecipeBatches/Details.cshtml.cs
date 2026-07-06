using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public PreparedRecipeBatchSummary PreparedRecipeBatchSummary { get; set; } = new();

        public RecipeNutritionTotals ItemTotals { get; set; } = new();

        public RecipeNutritionTotals ItemPerServingTotals { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .Include(preparedRecipeBatch => preparedRecipeBatch.Items)
                .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == id);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            decimal allocatedServings = preparedRecipeBatch.DailyPlanItems
                .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);

            PreparedRecipeBatchSummary = new PreparedRecipeBatchSummary
            {
                PreparedRecipeBatch = preparedRecipeBatch,
                AllocatedServings = allocatedServings
            };

            ItemTotals = new RecipeNutritionTotals
            {
                Calories = preparedRecipeBatch.Items.Sum(item => item.CaloriesSnapshot),
                Protein = preparedRecipeBatch.Items.Sum(item => item.ProteinSnapshot),
                Fiber = preparedRecipeBatch.Items.Sum(item => item.FiberSnapshot),
                Sugar = preparedRecipeBatch.Items.Sum(item => item.SugarSnapshot),
                Salt = preparedRecipeBatch.Items.Sum(item => item.SaltSnapshot)
            };

            if (preparedRecipeBatch.TotalServings > 0)
            {
                ItemPerServingTotals = new RecipeNutritionTotals
                {
                    Calories = ItemTotals.Calories / preparedRecipeBatch.TotalServings,
                    Protein = ItemTotals.Protein / preparedRecipeBatch.TotalServings,
                    Fiber = ItemTotals.Fiber / preparedRecipeBatch.TotalServings,
                    Sugar = ItemTotals.Sugar / preparedRecipeBatch.TotalServings,
                    Salt = ItemTotals.Salt / preparedRecipeBatch.TotalServings
                };
            }

            return Page();
        }
    }
}
