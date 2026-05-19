using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Menu> Menus { get; set; } = [];

        public async Task OnGetAsync()
        {
            Menus = await _context.Menus
                .OrderBy(menu => menu.Date)
                .ToListAsync();
        }
    }
}
