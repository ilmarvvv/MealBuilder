using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly CurrentUserAccessor _currentUser;

        public DeleteModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public PreparedRecipeBatchSummary PreparedRecipeBatchSummary { get; set; } = new();

        public bool CanDelete => PreparedRecipeBatchSummary.AllocatedServings == 0;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .FirstOrDefaultAsync(preparedRecipeBatch =>
                    preparedRecipeBatch.Id == id &&
                    preparedRecipeBatch.OwnerId == _currentUser.UserId);

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
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .FirstOrDefaultAsync(preparedRecipeBatch =>
                    preparedRecipeBatch.Id == id &&
                    preparedRecipeBatch.OwnerId == _currentUser.UserId);

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
            return preparedRecipeBatch.DailyPlanItems
                .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value);
        }
    }
}
