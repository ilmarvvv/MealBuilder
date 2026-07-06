using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<DailyPlan> DailyPlans { get; set; } = [];

        public async Task OnGetAsync()
        {
            DailyPlans = await _context.DailyPlans
                .OrderBy(dailyPlan => dailyPlan.Date)
                .ToListAsync();
        }
    }
}
