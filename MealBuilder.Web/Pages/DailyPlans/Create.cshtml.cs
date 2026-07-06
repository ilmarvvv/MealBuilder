using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DailyPlan DailyPlan { get; set; } = new();

        public void OnGet(DateOnly? date)
        {
            DailyPlan.Date = date ?? DateOnly.FromDateTime(DateTime.Today);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool dailyPlanDateAlreadyExists = await _context.DailyPlans
                .AnyAsync(dailyPlan => dailyPlan.Date == DailyPlan.Date);

            if (dailyPlanDateAlreadyExists)
            {
                ModelState.AddModelError("DailyPlan.Date", "A daily plan already exists for this date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DailyPlans.Add(DailyPlan);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
