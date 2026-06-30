using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public DailyPlan DailyPlan { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(id);

            if (dailyPlan is null)
            {
                return NotFound();
            }

            DailyPlan = dailyPlan;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            DailyPlan? dailyPlan = await _context.DailyPlans.FindAsync(id);

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
