using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly MenuCalculationService _menuCalculationService;

        public DetailsModel(
            AppDbContext context,
            MenuCalculationService menuCalculationService)
        {
            _context = context;
            _menuCalculationService = menuCalculationService;
        }

        public Menu Menu { get; set; } = new();

        public RecipeNutritionTotals Totals { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu? menu = await _context.Menus
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Recipe)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.PreparedRecipeBatch)
                .ThenInclude(preparedRecipeBatch => preparedRecipeBatch!.Items)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Recipe)
                .ThenInclude(recipe => recipe!.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(menu => menu.MenuItems)
                .ThenInclude(menuItem => menuItem.Recipe)
                .ThenInclude(recipe => recipe!.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(menu => menu.Id == id);

            if (menu is null)
            {
                return NotFound();
            }

            Menu = menu;
            Totals = _menuCalculationService.Calculate(menu);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveItemAsync(int menuItemId)
        {
            MenuItem? menuItem = await _context.MenuItems
                .FindAsync(menuItemId);

            if (menuItem is null)
            {
                return NotFound();
            }

            int menuId = menuItem.MenuId;

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = menuId });
        }
    }
}
