using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class AddIngredientModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddIngredientModel(AppDbContext context)
        {
            _context = context;
        }

        public Menu Menu { get; set; } = new();

        public SelectList Ingredients { get; set; } = null!;

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
            MenuItem.ItemType = MenuItemType.Ingredient;

            await LoadIngredientsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("MenuItem.Menu");
            ModelState.Remove("MenuItem.Recipe");
            ModelState.Remove("MenuItem.Ingredient");

            MenuItem.ItemType = MenuItemType.Ingredient;
            MenuItem.RecipeId = null;
            MenuItem.ServingsCount = null;

            if (MenuItem.IngredientId is null)
            {
                ModelState.AddModelError("MenuItem.IngredientId", "Ingredient is required.");
            }

            if (MenuItem.Grams is null || MenuItem.Grams <= 0)
            {
                ModelState.AddModelError("MenuItem.Grams", "Grams must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                Menu? menu = await _context.Menus.FindAsync(MenuItem.MenuId);

                if (menu is not null)
                {
                    Menu = menu;
                }

                await LoadIngredientsAsync();
                return Page();
            }

            _context.MenuItems.Add(MenuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = MenuItem.MenuId });
        }

        private async Task LoadIngredientsAsync()
        {
            List<Ingredient> ingredients = await _context.Ingredients
                .OrderBy(ingredient => ingredient.Name)
                .ToListAsync();

            Ingredients = new SelectList(ingredients, "Id", "Name");
        }
    }
}
