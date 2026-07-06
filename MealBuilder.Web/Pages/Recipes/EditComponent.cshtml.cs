using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.Recipes
{
    public class EditComponentModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditComponentModel(AppDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public RecipeComponent RecipeComponent { get; set; } = new();

        public string ComponentRecipeName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            RecipeComponent? recipeComponent = await _context.RecipeComponents
                .Include(recipeComponent => recipeComponent.ComponentRecipe)
                .FirstOrDefaultAsync(recipeComponent => recipeComponent.Id == id);

            if (recipeComponent is null)
            {
                return NotFound();
            }

            RecipeComponent = recipeComponent;
            ComponentRecipeName = recipeComponent.ComponentRecipe.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("RecipeComponent.ParentRecipe");
            ModelState.Remove("RecipeComponent.ComponentRecipe");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.RecipeComponents.Update(RecipeComponent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = RecipeComponent.ParentRecipeId });
        }
    }
}
