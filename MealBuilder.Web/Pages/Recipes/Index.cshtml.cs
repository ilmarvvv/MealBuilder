using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Recipe> Recipes { get; set; } = [];

        public async Task OnGetAsync()
        {
            Recipes = await _context.Recipes
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();
        }
    }
}
