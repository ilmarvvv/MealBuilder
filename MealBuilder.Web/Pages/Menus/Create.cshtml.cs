using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Menu Menu { get; set; } = new();

        public void OnGet(DateOnly? date)
        {
            Menu.Date = date ?? DateOnly.FromDateTime(DateTime.Today);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool menuDateAlreadyExists = await _context.Menus
                .AnyAsync(menu => menu.Date == Menu.Date);

            if (menuDateAlreadyExists)
            {
                ModelState.AddModelError("Menu.Date", "A menu already exists for this date.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Menus.Add(Menu);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
