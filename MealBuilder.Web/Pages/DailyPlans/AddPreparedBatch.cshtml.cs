using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class AddPreparedBatchModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddPreparedBatchModel(AppDbContext context)
        {
            _context = context;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        public SelectList PreparedRecipeBatches { get; set; } = null!;

        [BindProperty]
        public DailyPlanItem DailyPlanItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(id);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            DailyPlan = dailyPlan;
            DailyPlanItem.DailyPlanId = dailyPlan.Id;
            DailyPlanItem.ItemType = DailyPlanItemType.PreparedRecipeBatch;

            await LoadPreparedRecipeBatchesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("DailyPlanItem.DailyPlan");
            ModelState.Remove("DailyPlanItem.Recipe");
            ModelState.Remove("DailyPlanItem.Ingredient");
            ModelState.Remove("DailyPlanItem.PreparedRecipeBatch");

            DailyPlanItem.ItemType = DailyPlanItemType.PreparedRecipeBatch;
            DailyPlanItem.RecipeId = null;
            DailyPlanItem.IngredientId = null;
            DailyPlanItem.Grams = null;

            if (DailyPlanItem.PreparedRecipeBatchId is null)
            {
                ModelState.AddModelError("DailyPlanItem.PreparedRecipeBatchId", "Prepared batch is required.");
            }

            if (DailyPlanItem.ServingsCount is null || DailyPlanItem.ServingsCount <= 0)
            {
                ModelState.AddModelError("DailyPlanItem.ServingsCount", "Servings count must be greater than 0.");
            }

            if (DailyPlanItem.PreparedRecipeBatchId is not null && DailyPlanItem.ServingsCount is not null)
            {
                PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                    .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                    .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == DailyPlanItem.PreparedRecipeBatchId);

                if (preparedRecipeBatch is null)
                {
                    ModelState.AddModelError("DailyPlanItem.PreparedRecipeBatchId", "Prepared batch was not found.");
                }
                else
                {
                    decimal allocatedServings = preparedRecipeBatch.DailyPlanItems
                        .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                        .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);

                    decimal unallocatedServings =
                        preparedRecipeBatch.TotalServings - allocatedServings;

                    if (DailyPlanItem.ServingsCount > unallocatedServings)
                    {
                        ModelState.AddModelError(
                            "DailyPlanItem.ServingsCount",
                            $"Only {unallocatedServings} unallocated servings are available for this prepared batch.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(DailyPlanItem.DailyPlanId);

                if (dailyPlan is not null)
                {
                    DailyPlan = dailyPlan;
                }

                await LoadPreparedRecipeBatchesAsync();
                return Page();
            }

            _context.DailyPlanItems.Add(DailyPlanItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = DailyPlanItem.DailyPlanId });
        }

        private async Task LoadPreparedRecipeBatchesAsync()
        {
            List<PreparedRecipeBatch> preparedRecipeBatches = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .OrderBy(preparedRecipeBatch => preparedRecipeBatch.RecipeNameSnapshot)
                .ThenByDescending(preparedRecipeBatch => preparedRecipeBatch.CookedDate)
                .ToListAsync();

            var availablePreparedRecipeBatches = preparedRecipeBatches
                .Select(preparedRecipeBatch =>
                {
                    decimal allocatedServings = preparedRecipeBatch.DailyPlanItems
                        .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                        .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);

                    decimal unallocatedServings =
                        preparedRecipeBatch.TotalServings - allocatedServings;

                    return new
                    {
                        preparedRecipeBatch.Id,

                        Label =
                            $"{preparedRecipeBatch.RecipeNameSnapshot} | " +
                            $"cooked {preparedRecipeBatch.CookedDate} | " +
                            $"unallocated {unallocatedServings} servings",

                        UnallocatedServings = unallocatedServings
                    };
                })
                .Where(preparedRecipeBatch =>
                    preparedRecipeBatch.UnallocatedServings > 0)
                .ToList();

            PreparedRecipeBatches = new SelectList(
                availablePreparedRecipeBatches,
                "Id",
                "Label");
        }
    }
}
