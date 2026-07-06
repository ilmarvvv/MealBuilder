using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class EditIngredientModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditIngredientModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RecipeIngredient RecipeIngredient { get; set; } = new();

        public string IngredientName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            RecipeIngredient? recipeIngredient = await _context.RecipeIngredients
                .Include(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipeIngredient => recipeIngredient.Id == id);

            if (recipeIngredient is null)
            {
                return NotFound();
            }

            RecipeIngredient = recipeIngredient;
            IngredientName = recipeIngredient.Ingredient.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("RecipeIngredient.Recipe");
            ModelState.Remove("RecipeIngredient.Ingredient");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.RecipeIngredients.Update(RecipeIngredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = RecipeIngredient.RecipeId });
        }
    }
}
