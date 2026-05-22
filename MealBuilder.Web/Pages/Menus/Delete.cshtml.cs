using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealBuilder.Web.Pages.Menus
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Menu? menu = await _context.Menus.FindAsync(id);

            if (menu is null)
            {
                return NotFound();
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
