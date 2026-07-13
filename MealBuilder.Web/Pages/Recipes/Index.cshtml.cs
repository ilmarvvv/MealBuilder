using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserAccessor _currentUser;

        public IndexModel(AppDbContext context, CurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public List<Recipe> Recipes { get; set; } = [];

        public async Task OnGetAsync()
        {
            Recipes = await _context.Recipes
                .Where(recipe => recipe.OwnerId == _currentUser.UserId)
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();
        }
    }
}
