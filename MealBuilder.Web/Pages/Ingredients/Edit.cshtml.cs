using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Ingredients
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public EditModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [BindProperty]
        public Ingredient Ingredient { get; set; } = new();

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ingredientToUpdate = await _context.Ingredients
                .FirstOrDefaultAsync(ingredient =>
                    ingredient.Id == Ingredient.Id &&
                    ingredient.OwnerId == _currentUser.UserId);

            if (ingredientToUpdate is null)
            {
                return NotFound();
            }

            ingredientToUpdate.Name = Ingredient.Name;
            ingredientToUpdate.CaloriesPer100g = Ingredient.CaloriesPer100g;
            ingredientToUpdate.ProteinPer100g = Ingredient.ProteinPer100g;
            ingredientToUpdate.FiberPer100g = Ingredient.FiberPer100g;
            ingredientToUpdate.SugarPer100g = Ingredient.SugarPer100g;
            ingredientToUpdate.SaltPer100g = Ingredient.SaltPer100g;
            ingredientToUpdate.GramsPerPiece = Ingredient.GramsPerPiece;
            ingredientToUpdate.GramsPerMilliliter = Ingredient.GramsPerMilliliter;
            ingredientToUpdate.Notes = Ingredient.Notes;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
