using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealBuilder.Web.Pages.Ingredients
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public Ingredient Ingredient { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);

            if (ingredient is null)
            {
                return NotFound();
            }

            Ingredient = ingredient;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);

            if (ingredient is null)
            {
                return NotFound();
            }

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
