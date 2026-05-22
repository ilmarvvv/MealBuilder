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

            decimal usedServings = preparedRecipeBatch.MenuItems
                .Where(menuItem => menuItem.ServingsCount is not null)
                .Sum(menuItem => menuItem.ServingsCount!.Value);

            PreparedRecipeBatchSummary = new PreparedRecipeBatchSummary
            {
                PreparedRecipeBatch = preparedRecipeBatch,
                UsedServings = usedServings
            };

            return Page();
        }
    }
}
