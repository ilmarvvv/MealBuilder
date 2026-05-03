using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Ingredients;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<Ingredient> Ingredients { get; set; } = [];

    public async Task OnGetAsync()
    {
        Ingredients = await _context.Ingredients
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync();
    }
}
