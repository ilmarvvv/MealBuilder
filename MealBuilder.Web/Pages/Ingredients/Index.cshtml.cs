using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Ingredients;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly CurrentUserAccessor _currentUser;

    public IndexModel(AppDbContext context, CurrentUserAccessor currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public List<Ingredient> Ingredients { get; set; } = [];

    public async Task OnGetAsync()
    {
        Ingredients = await _context.Ingredients
            .Where(ingredient => ingredient.OwnerId == _currentUser.UserId)
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync();
    }
}
