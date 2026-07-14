using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public EditModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [BindProperty]
        public DailyPlan DailyPlan { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, DateOnly? date)
        {
            if (id.HasValue)
            {
                DailyPlan? dailyPlan = await _context.DailyPlans
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Id == id.Value &&
                        dailyPlan.OwnerId == _currentUser.UserId);

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
                    dailyPlan.Date == date.Value &&
                    dailyPlan.OwnerId == _currentUser.UserId);

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
                    dailyPlan.Id != DailyPlan.Id &&
                    dailyPlan.OwnerId == _currentUser.UserId);

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
                    .FirstOrDefaultAsync(dailyPlan =>
                        dailyPlan.Id == DailyPlan.Id &&
                        dailyPlan.OwnerId == _currentUser.UserId);

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
                DailyPlan.OwnerId = _currentUser.UserId;

                dailyPlan = DailyPlan;
                _context.DailyPlans.Add(dailyPlan);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = dailyPlan.Id });
        }
    }
}
