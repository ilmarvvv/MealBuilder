using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealBuilder.Web.Pages.Recipes
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public Recipe Recipe { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

            if (recipe is null)
            {
                return NotFound();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
