using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            bool dailyPlanDateAlreadyExists = await _context.DailyPlans
                .AnyAsync(dailyPlan => dailyPlan.Date == DailyPlan.Date && dailyPlan.Id != DailyPlan.Id);

            if (dailyPlanDateAlreadyExists)
            {
                ModelState.AddModelError("DailyPlan.Date", "A daily plan already exists for this date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(DailyPlan).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = DailyPlan.Id });
        }
    }
}
