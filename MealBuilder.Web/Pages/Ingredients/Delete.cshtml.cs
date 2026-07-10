using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Ingredients
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public DeleteModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public Ingredient Ingredient { get; set; } = new();

        public bool IsUsed { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(ingredient =>
                    ingredient.Id == id &&
                    ingredient.OwnerId == _currentUser.UserId);

            if (ingredient is null)
            {
                return NotFound();
            }

            Ingredient = ingredient;
            IsUsed = await IsIngredientUsedAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(ingredient =>
                    ingredient.Id == id &&
                    ingredient.OwnerId == _currentUser.UserId);

            if (ingredient is null)
            {
                return NotFound();
            }

            if (await IsIngredientUsedAsync(id))
            {
                Ingredient = ingredient;
                IsUsed = true;

                return Page();
            }

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task<bool> IsIngredientUsedAsync(int ingredientId)
        {
            return await _context.RecipeIngredients.AnyAsync(recipeIngredient =>
                       recipeIngredient.IngredientId == ingredientId)
                   || await _context.DailyPlanItems.AnyAsync(dailyPlanItem =>
                       dailyPlanItem.IngredientId == ingredientId);
        }
    }
}
