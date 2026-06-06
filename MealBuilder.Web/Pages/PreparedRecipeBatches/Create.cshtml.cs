using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        private readonly RecipeCalculationService _recipeCalculationService;

        public CreateModel(AppDbContext context, RecipeCalculationService recipeCalculationService)
        {
            _context = context;
            _recipeCalculationService = recipeCalculationService;
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
            ModelState.Remove("PreparedRecipeBatch.RecipeNameSnapshot");
            ModelState.Remove("PreparedRecipeBatch.TotalCaloriesSnapshot");
            ModelState.Remove("PreparedRecipeBatch.TotalProteinSnapshot");
            ModelState.Remove("PreparedRecipeBatch.TotalFiberSnapshot");
            ModelState.Remove("PreparedRecipeBatch.TotalSugarSnapshot");
            ModelState.Remove("PreparedRecipeBatch.TotalSaltSnapshot");

            if (!ModelState.IsValid)
            {
                await LoadRecipeSelectListAsync();
                return Page();
            }

            Recipe? recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(recipe => recipe.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipe => recipe.Id == PreparedRecipeBatch.RecipeId);

            if (recipe is null)
            {
                ModelState.AddModelError("PreparedRecipeBatch.RecipeId", "Recipe was not found.");
                await LoadRecipeSelectListAsync();

                return Page();
            }

            RecipeNutritionTotals totals = _recipeCalculationService.Calculate(recipe);

            PreparedRecipeBatch.RecipeNameSnapshot = recipe.Name;
            PreparedRecipeBatch.TotalCaloriesSnapshot = totals.Calories;
            PreparedRecipeBatch.TotalProteinSnapshot = totals.Protein;
            PreparedRecipeBatch.TotalFiberSnapshot = totals.Fiber;
            PreparedRecipeBatch.TotalSugarSnapshot = totals.Sugar;
            PreparedRecipeBatch.TotalSaltSnapshot = totals.Salt;

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
