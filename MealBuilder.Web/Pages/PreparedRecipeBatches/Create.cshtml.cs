using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public SelectList RecipeSelectList { get; set; } = default!;

        public async Task OnGetAsync(int? recipeId)
        {
            PreparedRecipeBatch.CookedDate = DateOnly.FromDateTime(DateTime.Today);

            if (recipeId is not null)
            {
                PreparedRecipeBatch.RecipeId = recipeId.Value;
            }

            await LoadRecipeSelectListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRecipeSelectListAsync();
                return Page();
            }

            _context.PreparedRecipeBatches.Add(PreparedRecipeBatch);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadRecipeSelectListAsync()
        {
            List<Recipe> recipes = await _context.Recipes
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();

            RecipeSelectList = new SelectList(recipes, "Id", "Name");
        }
    }
}
