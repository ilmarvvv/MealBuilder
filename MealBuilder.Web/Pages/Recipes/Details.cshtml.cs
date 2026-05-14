using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Recipe Recipe { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipe => recipe.Id == id);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveIngredientAsync(int recipeIngredientId)
        {
            RecipeIngredient? recipeIngredient = await _context.RecipeIngredients
                .FindAsync(recipeIngredientId);

            if (recipeIngredient is null)
            {
                return NotFound();
            }

            int recipeId = recipeIngredient.RecipeId;

            _context.RecipeIngredients.Remove(recipeIngredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = recipeId });
        }
    }
}
