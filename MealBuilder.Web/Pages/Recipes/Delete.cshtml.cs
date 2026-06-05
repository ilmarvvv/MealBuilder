using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

        public bool IsUsed { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;
            IsUsed = await IsRecipeUsedAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Recipe? recipe = await _context.Recipes.FindAsync(id);

            if (recipe is null)
            {
                return NotFound();
            }

            if (await IsRecipeUsedAsync(id))
            {
                Recipe = recipe;
                IsUsed = true;

                return Page();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task<bool> IsRecipeUsedAsync(int recipeId)
        {
            return await _context.RecipeComponents.AnyAsync(recipeComponent =>
                       recipeComponent.ParentRecipeId == recipeId
                       || recipeComponent.ComponentRecipeId == recipeId)
                   || await _context.MenuItems.AnyAsync(menuItem =>
                       menuItem.RecipeId == recipeId)
                   || await _context.PreparedRecipeBatches.AnyAsync(preparedRecipeBatch =>
                       preparedRecipeBatch.RecipeId == recipeId);
        }
    }
}
