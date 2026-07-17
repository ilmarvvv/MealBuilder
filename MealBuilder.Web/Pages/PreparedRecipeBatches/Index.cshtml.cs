using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly CurrentUserAccessor _currentUser;

        public IndexModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public List<PreparedRecipeBatchSummary> PreparedRecipeBatchSummaries { get; set; } = [];

        public async Task OnGetAsync()
        {
            List<PreparedRecipeBatch> preparedRecipeBatches = await _context.PreparedRecipeBatches
                .Where(preparedRecipeBatch => preparedRecipeBatch.OwnerId == _currentUser.UserId)
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.DailyPlanItems)
                .OrderByDescending(preparedRecipeBatch => preparedRecipeBatch.CookedDate)
                .ToListAsync();

            PreparedRecipeBatchSummaries = preparedRecipeBatches
                .Select(preparedRecipeBatch => new PreparedRecipeBatchSummary
                {
                    PreparedRecipeBatch = preparedRecipeBatch,
                    AllocatedServings = preparedRecipeBatch.DailyPlanItems
                        .Where(dailyPlanItem => dailyPlanItem.ServingsCount is not null)
                        .Sum(dailyPlanItem => dailyPlanItem.ServingsCount!.Value)
                })
                .ToList();
        }
    }
}
