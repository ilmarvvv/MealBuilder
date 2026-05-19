using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Menu Menu { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu? menu = await _context.Menus
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Recipe)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Ingredient)
                .FirstOrDefaultAsync(menu => menu.Id == id);

            if (menu is null)
            {
                return NotFound();
            }

            Menu = menu;

            return Page();
        }
    }
}
