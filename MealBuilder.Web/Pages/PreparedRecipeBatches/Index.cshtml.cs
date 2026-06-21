using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<PreparedRecipeBatchSummary> PreparedRecipeBatchSummaries { get; set; } = [];

        public async Task OnGetAsync()
        {
            List<PreparedRecipeBatch> preparedRecipeBatches = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .OrderByDescending(preparedRecipeBatch => preparedRecipeBatch.CookedDate)
                .ToListAsync();

            PreparedRecipeBatchSummaries = preparedRecipeBatches
                .Select(preparedRecipeBatch => new PreparedRecipeBatchSummary
                {
                    PreparedRecipeBatch = preparedRecipeBatch,
                    AllocatedServings = preparedRecipeBatch.MenuItems
                        .Where(menuItem => menuItem.ServingsCount is not null)
                        .Sum(menuItem => menuItem.ServingsCount!.Value)
                })
                .ToList();
        }
    }
}
