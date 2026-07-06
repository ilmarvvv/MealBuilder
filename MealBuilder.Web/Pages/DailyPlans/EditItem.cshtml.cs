using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class EditItemModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditItemModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DailyPlanItem DailyPlanItem { get; set; } = new();

        public string ItemName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlanItem? dailyPlanItem = await _context.DailyPlanItems
                .Include(dailyPlanItem => dailyPlanItem.Recipe)
                .Include(dailyPlanItem => dailyPlanItem.Ingredient)
                .Include(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .FirstOrDefaultAsync(dailyPlanItem => dailyPlanItem.Id == id);

            if (dailyPlanItem is null)
            {
                return NotFound();
            }

            DailyPlanItem = dailyPlanItem;
            ItemName = GetItemName(dailyPlanItem);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("DailyPlanItem.DailyPlan");
            ModelState.Remove("DailyPlanItem.Recipe");
            ModelState.Remove("DailyPlanItem.Ingredient");
            ModelState.Remove("DailyPlanItem.PreparedRecipeBatch");

            if (DailyPlanItem.ItemType == DailyPlanItemType.Recipe)
            {
                DailyPlanItem.IngredientId = null;
                DailyPlanItem.Grams = null;

                if (DailyPlanItem.ServingsCount is null || DailyPlanItem.ServingsCount <= 0)
                {
                    ModelState.AddModelError("DailyPlanItem.ServingsCount", "Servings count must be greater than 0.");
                }
            }
            else if (DailyPlanItem.ItemType == DailyPlanItemType.PreparedRecipeBatch)
            {
                DailyPlanItem.RecipeId = null;
                DailyPlanItem.IngredientId = null;
                DailyPlanItem.Grams = null;

                if (DailyPlanItem.ServingsCount is null || DailyPlanItem.ServingsCount <= 0)
                {
                    ModelState.AddModelError("DailyPlanItem.ServingsCount", "Servings count must be greater than 0.");
                }

                await ValidatePreparedBatchServingsAsync();
            }
            else if (DailyPlanItem.ItemType == DailyPlanItemType.Ingredient)
            {
                DailyPlanItem.RecipeId = null;
                DailyPlanItem.ServingsCount = null;

                if (DailyPlanItem.Grams is null || DailyPlanItem.Grams <= 0)
                {
                    ModelState.AddModelError("DailyPlanItem.Grams", "Grams must be greater than 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                DailyPlanItem? existingDailyPlanItem = await _context.DailyPlanItems
                    .Include(dailyPlanItem => dailyPlanItem.Recipe)
                    .Include(dailyPlanItem => dailyPlanItem.Ingredient)
                    .Include(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                    .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                    .FirstOrDefaultAsync(dailyPlanItem => dailyPlanItem.Id == DailyPlanItem.Id);

                if (existingDailyPlanItem is not null)
                {
                    ItemName = GetItemName(existingDailyPlanItem);
                }

                return Page();
            }

            _context.DailyPlanItems.Update(DailyPlanItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = DailyPlanItem.DailyPlanId });
        }

        private async Task ValidatePreparedBatchServingsAsync()
        {
            if (DailyPlanItem.PreparedRecipeBatchId is null || DailyPlanItem.ServingsCount is null)
            {
                return;
            }

            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == DailyPlanItem.PreparedRecipeBatchId);

            if (preparedRecipeBatch is null)
            {
                ModelState.AddModelError("DailyPlanItem.PreparedRecipeBatchId", "Prepared batch was not found.");
                return;
            }

            decimal allocatedByOtherDailyPlanItems = preparedRecipeBatch.DailyPlanItems
                .Where(dailyPlanItem => dailyPlanItem.Id != DailyPlanItem.Id &&
                       dailyPlanItem.ServingsCount is not null)
                .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);

            decimal availableServings =
                preparedRecipeBatch.TotalServings -
                allocatedByOtherDailyPlanItems;

            if (DailyPlanItem.ServingsCount > availableServings)
            {
                ModelState.AddModelError("DailyPlanItem.ServingsCount", $"Only {availableServings} servings are available for this prepared batch.");
            }
        }

        private static string GetItemName(DailyPlanItem dailyPlanItem)
        {
            if (dailyPlanItem.ItemType == DailyPlanItemType.Recipe)
            {
                return dailyPlanItem.Recipe?.Name ?? string.Empty;
            }

            if (dailyPlanItem.ItemType == DailyPlanItemType.Ingredient)
            {
                return dailyPlanItem.Ingredient?.Name ?? string.Empty;
            }

            if (dailyPlanItem.ItemType == DailyPlanItemType.PreparedRecipeBatch)
            {
                return $"{dailyPlanItem.PreparedRecipeBatch?.RecipeNameSnapshot} | cooked {dailyPlanItem.PreparedRecipeBatch?.CookedDate}";
            }

            return string.Empty;
        }
    }
}
