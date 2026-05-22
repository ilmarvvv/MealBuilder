using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Menus
{
    public class AddPreparedBatchModel : PageModel
    {
        private readonly AppDbContext _context;

        public AddPreparedBatchModel(AppDbContext context)
        {
            _context = context;
        }

        public Menu Menu { get; set; } = new();

        public SelectList PreparedRecipeBatches { get; set; } = null!;

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
            MenuItem.ItemType = MenuItemType.PreparedRecipeBatch;

            await LoadPreparedRecipeBatchesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("MenuItem.Menu");
            ModelState.Remove("MenuItem.Recipe");
            ModelState.Remove("MenuItem.Ingredient");
            ModelState.Remove("MenuItem.PreparedRecipeBatch");

            MenuItem.ItemType = MenuItemType.PreparedRecipeBatch;
            MenuItem.RecipeId = null;
            MenuItem.IngredientId = null;
            MenuItem.Grams = null;

            if (MenuItem.PreparedRecipeBatchId is null)
            {
                ModelState.AddModelError("MenuItem.PreparedRecipeBatchId", "Prepared batch is required.");
            }

            if (MenuItem.ServingsCount is null || MenuItem.ServingsCount <= 0)
            {
                ModelState.AddModelError("MenuItem.ServingsCount", "Servings count must be greater than 0.");
            }

            if (MenuItem.PreparedRecipeBatchId is not null && MenuItem.ServingsCount is not null)
            {
                PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                    .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                    .FirstOrDefaultAsync(preparedRecipeBatch => preparedRecipeBatch.Id == MenuItem.PreparedRecipeBatchId);

                if (preparedRecipeBatch is null)
                {
                    ModelState.AddModelError("MenuItem.PreparedRecipeBatchId", "Prepared batch was not found.");
                }
                else
                {
                    decimal usedServings = preparedRecipeBatch.MenuItems
                        .Where(menuItem => menuItem.ServingsCount is not null)
                        .Sum(menuItem => menuItem.ServingsCount!.Value);

                    decimal remainingServings = preparedRecipeBatch.TotalServings - usedServings;

                    if (MenuItem.ServingsCount > remainingServings)
                    {
                        ModelState.AddModelError("MenuItem.ServingsCount", $"Only {remainingServings} servings remain in this prepared batch.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                Menu? menu = await _context.Menus.FindAsync(MenuItem.MenuId);

                if (menu is not null)
                {
                    Menu = menu;
                }

                await LoadPreparedRecipeBatchesAsync();
                return Page();
            }

            _context.MenuItems.Add(MenuItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = MenuItem.MenuId });
        }

        private async Task LoadPreparedRecipeBatchesAsync()
        {
            List<PreparedRecipeBatch> preparedRecipeBatches = await _context.PreparedRecipeBatches
                .Include(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .Include(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .OrderBy(preparedRecipeBatch => preparedRecipeBatch.Recipe!.Name)
                .ThenByDescending(preparedRecipeBatch => preparedRecipeBatch.CookedDate)
                .ToListAsync();

            List<PreparedRecipeBatch> availablePreparedRecipeBatches = preparedRecipeBatches
                .Where(preparedRecipeBatch =>
                {
                    decimal usedServings = preparedRecipeBatch.MenuItems
                        .Where(menuItem => menuItem.ServingsCount is not null)
                        .Sum(menuItem => menuItem.ServingsCount!.Value);

                    decimal remainingServings = preparedRecipeBatch.TotalServings - usedServings;

                    return remainingServings > 0;
                })
                .ToList();

            PreparedRecipeBatches = new SelectList(
                availablePreparedRecipeBatches,
                "Id",
                "Recipe.Name");
        }
    }
}
