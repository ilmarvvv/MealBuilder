using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
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

        public DailyPlan DailyPlan { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans
                .FirstOrDefaultAsync(dailyPlan =>
                    dailyPlan.Id == id &&
                    dailyPlan.OwnerId == _currentUser.UserId);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            DailyPlan = dailyPlan;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans
                .FirstOrDefaultAsync(dailyPlan =>
                    dailyPlan.Id == id &&
                    dailyPlan.OwnerId == _currentUser.UserId);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            _context.DailyPlans.Remove(dailyPlan);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
