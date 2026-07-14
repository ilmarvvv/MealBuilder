using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.DailyPlans
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

        public List<DailyPlan> DailyPlans { get; set; } = [];

        public async Task OnGetAsync()
        {
            DailyPlans = await _context.DailyPlans
                .Where(dailyPlan => dailyPlan.OwnerId == _currentUser.UserId)
                .OrderBy(dailyPlan => dailyPlan.Date)
                .ToListAsync();
        }
    }
}
