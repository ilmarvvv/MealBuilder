using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Ingredients
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public DetailsModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

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
    }
}
