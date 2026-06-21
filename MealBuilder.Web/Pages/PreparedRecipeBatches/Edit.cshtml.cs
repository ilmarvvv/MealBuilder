using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
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
                .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .Include(preparedRecipeBatch => preparedRecipeBatch.Items)
                .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == id);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            LoadPreparedRecipeBatchSummary(preparedRecipeBatch);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int preparedRecipeBatchItemId)
        {
            PreparedRecipeBatchItem? item = await _context.PreparedRecipeBatchItems
                .FirstOrDefaultAsync(item => item.Id == preparedRecipeBatchItemId);

            if (item is null)
            {
                return NotFound();
            }

            int preparedRecipeBatchId = item.PreparedRecipeBatchId;

            _context.PreparedRecipeBatchItems.Remove(item);
            await _context.SaveChangesAsync();

            await ReorderBatchItemsAsync(preparedRecipeBatchId);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = preparedRecipeBatchId });
        }

        public async Task<IActionResult> OnPostChangeItemPositionAsync(
            int preparedRecipeBatchItemId,
            int newPosition)
        {
            PreparedRecipeBatchItem? item = await _context.PreparedRecipeBatchItems
                .FirstOrDefaultAsync(item => item.Id == preparedRecipeBatchItemId);

            if (item is null)
            {
                return NotFound();
            }

            int preparedRecipeBatchId = item.PreparedRecipeBatchId;

            List<PreparedRecipeBatchItem> items = await _context.PreparedRecipeBatchItems
                .Where(batchItem => batchItem.PreparedRecipeBatchId == preparedRecipeBatchId)
                .OrderBy(batchItem => batchItem.Position)
                .ThenBy(batchItem => batchItem.Id)
                .ToListAsync();

            items.Remove(item);

            int insertIndex = Math.Clamp(newPosition - 1, 0, items.Count);
            items.Insert(insertIndex, item);

            for (int index = 0; index < items.Count; index++)
            {
                items[index].Position = index + 1;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = preparedRecipeBatchId });
        }

        private void LoadPreparedRecipeBatchSummary(PreparedRecipeBatch preparedRecipeBatch)
        {
            decimal allocatedServings = preparedRecipeBatch.MenuItems
                .Where(menuItem => menuItem.ServingsCount is not null)
                .Sum(menuItem => menuItem.ServingsCount!.Value);

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
        }

        private async Task ReorderBatchItemsAsync(int preparedRecipeBatchId)
        {
            List<PreparedRecipeBatchItem> items = await _context.PreparedRecipeBatchItems
                .Where(item => item.PreparedRecipeBatchId == preparedRecipeBatchId)
                .OrderBy(item => item.Position)
                .ThenBy(item => item.Id)
                .ToListAsync();

            for (int index = 0; index < items.Count; index++)
            {
                items[index].Position = index + 1;
            }
        }
    }
}