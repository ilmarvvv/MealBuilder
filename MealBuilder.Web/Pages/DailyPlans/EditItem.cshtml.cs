using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class EditItemModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly CurrentUserAccessor _currentUser;

        public EditItemModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
                .FirstOrDefaultAsync(dailyPlanItem =>
                    dailyPlanItem.Id == id &&
                    dailyPlanItem.DailyPlan.OwnerId == _currentUser.UserId);

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
            int dailyPlanItemId = DailyPlanItem.Id;
            decimal? submittedServingsCount = DailyPlanItem.ServingsCount;
            decimal? submittedGrams = DailyPlanItem.Grams;

            DailyPlanItem? existingDailyPlanItem = await _context.DailyPlanItems
                .Include(dailyPlanItem => dailyPlanItem.Recipe)
                .Include(dailyPlanItem => dailyPlanItem.Ingredient)
                .Include(dailyPlanItem => dailyPlanItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .FirstOrDefaultAsync(dailyPlanItem =>
                    dailyPlanItem.Id == dailyPlanItemId &&
                    dailyPlanItem.DailyPlan.OwnerId == _currentUser.UserId);

            if (existingDailyPlanItem is null)
            {
                return NotFound();
            }

            DailyPlanItem = existingDailyPlanItem;
            ModelState.Clear();

            if (DailyPlanItem.ItemType == DailyPlanItemType.Recipe)
            {
                DailyPlanItem.ServingsCount = submittedServingsCount;

                if (submittedServingsCount is null || submittedServingsCount <= 0)
                {
                    ModelState.AddModelError(
                        "DailyPlanItem.ServingsCount",
                        "Servings count must be greater than 0.");
                }
            }
            else if (DailyPlanItem.ItemType == DailyPlanItemType.PreparedRecipeBatch)
            {
                DailyPlanItem.ServingsCount = submittedServingsCount;

                if (submittedServingsCount is null || submittedServingsCount <= 0)
                {
                    ModelState.AddModelError(
                        "DailyPlanItem.ServingsCount",
                        "Servings count must be greater than 0.");
                }
                else
                {
                    await ValidatePreparedBatchServingsAsync();
                }
            }
            else if (DailyPlanItem.ItemType == DailyPlanItemType.Ingredient)
            {
                DailyPlanItem.Grams = submittedGrams;

                if (submittedGrams is null || submittedGrams <= 0)
                {
                    ModelState.AddModelError(
                        "DailyPlanItem.Grams",
                        "Grams must be greater than 0.");
                }
            }
            else
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ItemName = GetItemName(DailyPlanItem);
                return Page();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(
                "./Details",
                new { id = DailyPlanItem.DailyPlanId });
        }

        private async Task ValidatePreparedBatchServingsAsync()
        {
            if (DailyPlanItem.PreparedRecipeBatchId is null || DailyPlanItem.ServingsCount is null)
            {
                return;
            }

            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .FirstOrDefaultAsync(preparedRecipeBatch =>
                    preparedRecipeBatch.Id == DailyPlanItem.PreparedRecipeBatchId &&
                    preparedRecipeBatch.Recipe != null &&
                    preparedRecipeBatch.Recipe.OwnerId == _currentUser.UserId);

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
