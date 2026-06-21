using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public PreparedRecipeBatchSummary PreparedRecipeBatchSummary { get; set; } = new();

        public bool CanDelete => PreparedRecipeBatchSummary.AllocatedServings == 0;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == id);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            SetPreparedRecipeBatchSummary(preparedRecipeBatch);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == id);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            decimal allocatedServings = CalculateAllocatedServings(preparedRecipeBatch);

            if (allocatedServings > 0)
            {
                return RedirectToPage("./Details", new { id });
            }

            _context.PreparedRecipeBatches.Remove(preparedRecipeBatch);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void SetPreparedRecipeBatchSummary(PreparedRecipeBatch preparedRecipeBatch)
        {
            PreparedRecipeBatchSummary = new PreparedRecipeBatchSummary
            {
                PreparedRecipeBatch = preparedRecipeBatch,
                AllocatedServings = CalculateAllocatedServings(preparedRecipeBatch)
            };
        }

        private static decimal CalculateAllocatedServings(
            PreparedRecipeBatch preparedRecipeBatch)
        {
            return preparedRecipeBatch.MenuItems
                .Where(menuItem => menuItem.ServingsCount is not null)
                .Sum(menuItem => menuItem.ServingsCount!.Value);
        }
    }
}
