using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class AddRecipeModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddRecipeModel(AppDbContext context)
        {
            _context = context;
        }

        public Menu Menu { get; set; } = new();

        public SelectList Recipes { get; set; } = null!;

        [BindProperty]
        public MenuItem MenuItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu? menu = await _context.Menus.FindAsync(id);

            if (menu is null)
            {
                return NotFound();
            }

            Menu = menu;
            MenuItem.MenuId = menu.Id;
            MenuItem.ItemType = MenuItemType.Recipe;

            await LoadRecipesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("MenuItem.Menu");
            ModelState.Remove("MenuItem.Recipe");
            ModelState.Remove("MenuItem.Ingredient");

            MenuItem.ItemType = MenuItemType.Recipe;
            MenuItem.IngredientId = null;
            MenuItem.Grams = null;

            if (MenuItem.RecipeId is null)
            {
                ModelState.AddModelError("MenuItem.RecipeId", "Recipe is required.");
            }

            if (MenuItem.ServingsCount is null || MenuItem.ServingsCount <= 0)
            {
                ModelState.AddModelError("MenuItem.ServingsCount", "Servings count must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                Menu? menu = await _context.Menus.FindAsync(MenuItem.MenuId);

                if (menu is not null)
                {
                    Menu = menu;
                }

                await LoadRecipesAsync();
                return Page();
            }

            _context.MenuItems.Add(MenuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = MenuItem.MenuId });
        }

        private async Task LoadRecipesAsync()
        {
            List<Recipe> recipes = await _context.Recipes
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();

            Recipes = new SelectList(recipes, "Id", "Name");
        }
    }
}
