using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
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

        public PreparedRecipeBatchItem PreparedRecipeBatchItem { get; set; } = new();

        [BindProperty]
        public int PreparedRecipeBatchItemId { get; set; }

        [BindProperty]
        public decimal Grams { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatchItem? item = await _context.PreparedRecipeBatchItems
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.PreparedRecipeBatch != null &&
                    item.PreparedRecipeBatch.OwnerId == _currentUser.UserId);

            if (item is null)
            {
                return NotFound();
            }

            PreparedRecipeBatchItem = item;
            PreparedRecipeBatchItemId = item.Id;
            Grams = item.Grams;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PreparedRecipeBatchItem? existingItem = await _context.PreparedRecipeBatchItems
                .FirstOrDefaultAsync(item =>
                    item.Id == PreparedRecipeBatchItemId &&
                    item.PreparedRecipeBatch != null &&
                    item.PreparedRecipeBatch.OwnerId == _currentUser.UserId);

            if (existingItem is null)
            {
                return NotFound();
            }

            if (Grams < 0.01m || Grams > 100000m)
            {
                ModelState.AddModelError(nameof(Grams), "Grams must be between 0.01 and 100000.");

                PreparedRecipeBatchItem = existingItem;
                return Page();
            }

            decimal oldGrams = existingItem.Grams;
            decimal ratio = oldGrams > 0 ? Grams / oldGrams : 0;

            existingItem.Grams = Grams;
            existingItem.CaloriesSnapshot *= ratio;
            existingItem.ProteinSnapshot *= ratio;
            existingItem.FiberSnapshot *= ratio;
            existingItem.SugarSnapshot *= ratio;
            existingItem.SaltSnapshot *= ratio;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = existingItem.PreparedRecipeBatchId });
        }
    }
}