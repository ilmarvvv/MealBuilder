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

        public async Task<IActionResult> OnGetAsync(int? id, DateOnly? date)
        {
            if (id.HasValue)
            {
                DailyPlan? dailyPlan = await _context.DailyPlans
                    .FindAsync(id.Value);

                if (dailyPlan is null)
                {
                    return NotFound();
                }

                DailyPlan = dailyPlan;

                return Page();
            }

            if (!date.HasValue)
            {
                return BadRequest();
            }

            DailyPlan? existingDailyPlan = await _context.DailyPlans
                .FirstOrDefaultAsync(dailyPlan =>
                    dailyPlan.Date == date.Value);

            DailyPlan = existingDailyPlan ?? new DailyPlan
            {
                Date = date.Value,
                Name = $"Daily Plan {date.Value}"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool dailyPlanDateAlreadyExists = await _context.DailyPlans
                .AnyAsync(dailyPlan =>
                    dailyPlan.Date == DailyPlan.Date &&
                    dailyPlan.Id != DailyPlan.Id);

            if (dailyPlanDateAlreadyExists)
            {
                ModelState.AddModelError(
                    "DailyPlan.Date",
                    "A daily plan already exists for this date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            DailyPlan dailyPlan;

            if (DailyPlan.Id > 0)
            {
                DailyPlan? existingDailyPlan = await _context.DailyPlans
                    .FindAsync(DailyPlan.Id);

                if (existingDailyPlan is null)
                {
                    return NotFound();
                }

                existingDailyPlan.Name = DailyPlan.Name;
                existingDailyPlan.Date = DailyPlan.Date;
                existingDailyPlan.Description = DailyPlan.Description;

                dailyPlan = existingDailyPlan;
            }
            else
            {
                dailyPlan = DailyPlan;
                _context.DailyPlans.Add(dailyPlan);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = dailyPlan.Id });
        }
    }
}
