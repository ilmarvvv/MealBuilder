using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class EditItemModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditItemModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuItem MenuItem { get; set; } = new();

        public string ItemName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            MenuItem? menuItem = await _context.MenuItems
                .Include(menuItem => menuItem.Recipe)
                .Include(menuItem => menuItem.Ingredient)
                .Include(menuItem => menuItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .FirstOrDefaultAsync(menuItem => menuItem.Id == id);

            if (menuItem is null)
            {
                return NotFound();
            }

            MenuItem = menuItem;
            ItemName = GetItemName(menuItem);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("MenuItem.Menu");
            ModelState.Remove("MenuItem.Recipe");
            ModelState.Remove("MenuItem.Ingredient");
            ModelState.Remove("MenuItem.PreparedRecipeBatch");

            if (MenuItem.ItemType == MenuItemType.Recipe)
            {
                MenuItem.IngredientId = null;
                MenuItem.Grams = null;

                if (MenuItem.ServingsCount is null || MenuItem.ServingsCount <= 0)
                {
                    ModelState.AddModelError("MenuItem.ServingsCount", "Servings count must be greater than 0.");
                }
            }
            else if (MenuItem.ItemType == MenuItemType.PreparedRecipeBatch)
            {
                MenuItem.RecipeId = null;
                MenuItem.IngredientId = null;
                MenuItem.Grams = null;

                if (MenuItem.ServingsCount is null || MenuItem.ServingsCount <= 0)
                {
                    ModelState.AddModelError("MenuItem.ServingsCount", "Servings count must be greater than 0.");
                }
            }
            else if (MenuItem.ItemType == MenuItemType.Ingredient)
            {
                MenuItem.RecipeId = null;
                MenuItem.ServingsCount = null;

                if (MenuItem.Grams is null || MenuItem.Grams <= 0)
                {
                    ModelState.AddModelError("MenuItem.Grams", "Grams must be greater than 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                MenuItem? existingMenuItem = await _context.MenuItems
                    .Include(menuItem => menuItem.Recipe)
                    .Include(menuItem => menuItem.Ingredient)
                    .Include(menuItem => menuItem.PreparedRecipeBatch)
                    .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                    .FirstOrDefaultAsync(menuItem => menuItem.Id == MenuItem.Id);

                if (existingMenuItem is not null)
                {
                    ItemName = GetItemName(existingMenuItem);
                }

                return Page();
            }

            _context.MenuItems.Update(MenuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = MenuItem.MenuId });
        }

        private static string GetItemName(MenuItem menuItem)
        {
            if (menuItem.ItemType == MenuItemType.Recipe)
            {
                return menuItem.Recipe?.Name ?? string.Empty;
            }

            if (menuItem.ItemType == MenuItemType.Ingredient)
            {
                return menuItem.Ingredient?.Name ?? string.Empty;
            }

            if (menuItem.ItemType == MenuItemType.PreparedRecipeBatch)
            {
                return $"{menuItem.PreparedRecipeBatch?.Recipe?.Name} | cooked {menuItem.PreparedRecipeBatch?.CookedDate}";
            }

            return string.Empty;
        }
    }
}
