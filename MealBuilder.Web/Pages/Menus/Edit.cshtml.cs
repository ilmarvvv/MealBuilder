using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Menu Menu { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu? menu = await _context.Menus.FindAsync(id);

            if (menu is null)
            {
                return NotFound();
            }

            Menu = menu;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool menuDateAlreadyExists = await _context.Menus
                .AnyAsync(menu => menu.Date == Menu.Date && menu.Id != Menu.Id);

            if (menuDateAlreadyExists)
            {
                ModelState.AddModelError("Menu.Date", "A menu already exists for this date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Menu).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = Menu.Id });
        }
    }
}
