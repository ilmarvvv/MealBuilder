using MealBuilder.Web.Data;
using MealBuilder.Web.Identity;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Pages.PreparedRecipeBatches
{
    public class AddRecipeModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RecipeCalculationService _recipeCalculationService;
        private readonly CurrentUserAccessor _currentUser;

        public AddRecipeModel(
            AppDbContext context, 
            RecipeCalculationService recipeCalculationService, 
            CurrentUserAccessor currentUser)
        {
            _context = context;
            _recipeCalculationService = recipeCalculationService;
            _currentUser = currentUser;
        }

        public PreparedRecipeBatch PreparedRecipeBatch { get; set; } = new();

        public SelectList Recipes { get; set; } = default!;

        [BindProperty]
        public int PreparedRecipeBatchId { get; set; }

        [BindProperty]
        public int RecipeId { get; set; }

        [BindProperty]
        public decimal Grams { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .FirstOrDefaultAsync(batch =>
                    batch.Id == id &&
                    batch.OwnerId == _currentUser.UserId);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            PreparedRecipeBatch = preparedRecipeBatch;
            PreparedRecipeBatchId = preparedRecipeBatch.Id;

            await LoadRecipesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Grams < 0.01m || Grams > 100000m)
            {
                ModelState.AddModelError(nameof(Grams), "Grams must be between 0.01 and 100000.");
            }

            PreparedRecipeBatch? preparedRecipeBatch = await _context.PreparedRecipeBatches
                .Include(batch => batch.Items)
                .FirstOrDefaultAsync(batch =>
                    batch.Id == PreparedRecipeBatchId &&
                    batch.OwnerId == _currentUser.UserId);

            if (preparedRecipeBatch is null)
            {
                return NotFound();
            }

            Recipe? recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(recipe => recipe.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipe =>
                    recipe.Id == RecipeId &&
                    recipe.OwnerId == _currentUser.UserId);

            if (recipe is null)
            {
                ModelState.AddModelError(nameof(RecipeId), "Recipe was not found.");
            }

            if (!ModelState.IsValid || recipe is null)
            {
                PreparedRecipeBatch = preparedRecipeBatch;
                await LoadRecipesAsync();
                return Page();
            }

            decimal recipeWeight = _recipeCalculationService.CalculateEffectiveWeight(recipe);

            if (recipeWeight <= 0)
            {
                ModelState.AddModelError(nameof(RecipeId), "Recipe weight must be greater than 0.");
                PreparedRecipeBatch = preparedRecipeBatch;
                await LoadRecipesAsync();
                return Page();
            }

            RecipeNutritionTotals totals = _recipeCalculationService.Calculate(recipe);
            decimal ratio = Grams / recipeWeight;

            int nextPosition = preparedRecipeBatch.Items.Count == 0
                ? 1
                : preparedRecipeBatch.Items.Max(item => item.Position) + 1;

            PreparedRecipeBatchItem item = new()
            {
                PreparedRecipeBatchId = preparedRecipeBatch.Id,
                ItemType = PreparedRecipeBatchItemType.Recipe,
                SourceRecipeId = recipe.Id,
                NameSnapshot = recipe.Name,
                Grams = Grams,
                CaloriesSnapshot = totals.Calories * ratio,
                ProteinSnapshot = totals.Protein * ratio,
                FiberSnapshot = totals.Fiber * ratio,
                SugarSnapshot = totals.Sugar * ratio,
                SaltSnapshot = totals.Salt * ratio,
                Position = nextPosition
            };

            _context.PreparedRecipeBatchItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = preparedRecipeBatch.Id });
        }

        private async Task LoadRecipesAsync()
        {
            List<Recipe> recipes = await _context.Recipes
                .Where(recipe => recipe.OwnerId == _currentUser.UserId)
                .OrderBy(recipe => recipe.Name)
                .ToListAsync();

            Recipes = new SelectList(recipes, "Id", "Name");
        }
    }
}