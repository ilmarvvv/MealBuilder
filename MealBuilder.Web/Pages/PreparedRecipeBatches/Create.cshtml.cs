using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        [BindProperty]
        [Range(1, 100)]
        public int ServingsPerDay { get; set; } = 1;

        public SelectList RecipeSelectList { get; set; } = default!;

        public async Task OnGetAsync(int? recipeId)
        {
            PreparedRecipeBatch.CookedDate = DateOnly.FromDateTime(DateTime.Today);

            if (recipeId is not null)
            {
                Recipe? recipe = await _context.Recipes
                    .FirstOrDefaultAsync(recipe => recipe.Id == recipeId.Value);

                if (recipe is not null)
                {
                    PreparedRecipeBatch.RecipeId = recipe.Id;
                    PreparedRecipeBatch.PlannedDays = recipe.DefaultPlannedDays;
                    ServingsPerDay = recipe.DefaultServingsPerDay;
                    PreparedRecipeBatch.TotalServings = recipe.TotalServings;
                }
            }

            await LoadRecipeSelectListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("PreparedRecipeBatch.TotalServings");
            ModelState.Remove("PreparedRecipeBatch.RecipeNameSnapshot");

            PreparedRecipeBatch.TotalServings =
                PreparedRecipeBatch.PlannedDays * ServingsPerDay;

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
                ModelState.AddModelError(
                    "PreparedRecipeBatch.RecipeId",
                    "Recipe was not found.");

                await LoadRecipeSelectListAsync();
                return Page();
            }

            PreparedRecipeBatch.RecipeNameSnapshot = recipe.Name;

            PreparedRecipeBatch.Items = recipe.RecipeIngredients
                .Select(recipeIngredient => new PreparedRecipeBatchItem
                {
                    ItemType = PreparedRecipeBatchItemType.Ingredient,
                    SourceIngredientId = recipeIngredient.IngredientId,
                    NameSnapshot = recipeIngredient.Ingredient.Name,
                    Grams = recipeIngredient.Grams,
                    CaloriesSnapshot = recipeIngredient.Ingredient.CaloriesPer100g * recipeIngredient.Grams / 100,
                    ProteinSnapshot = recipeIngredient.Ingredient.ProteinPer100g * recipeIngredient.Grams / 100,
                    FiberSnapshot = recipeIngredient.Ingredient.FiberPer100g * recipeIngredient.Grams / 100,
                    SugarSnapshot = recipeIngredient.Ingredient.SugarPer100g * recipeIngredient.Grams / 100,
                    SaltSnapshot = recipeIngredient.Ingredient.SaltPer100g * recipeIngredient.Grams / 100,
                    Position = recipeIngredient.Position
                })
                .Concat(recipe.Components.Select(recipeComponent =>
                {
                    decimal componentTotalWeight = _recipeCalculationService.CalculateEffectiveWeight(recipeComponent.ComponentRecipe);

                    RecipeNutritionTotals componentTotals = _recipeCalculationService.Calculate(recipeComponent.ComponentRecipe);

                    decimal ratio = componentTotalWeight > 0
                        ? recipeComponent.Grams / componentTotalWeight
                        : 0;

                    return new PreparedRecipeBatchItem
                    {
                        ItemType = PreparedRecipeBatchItemType.Recipe,
                        SourceRecipeId = recipeComponent.ComponentRecipeId,
                        NameSnapshot = recipeComponent.ComponentRecipe.Name,
                        Grams = recipeComponent.Grams,
                        CaloriesSnapshot = componentTotals.Calories * ratio,
                        ProteinSnapshot = componentTotals.Protein * ratio,
                        FiberSnapshot = componentTotals.Fiber * ratio,
                        SugarSnapshot = componentTotals.Sugar * ratio,
                        SaltSnapshot = componentTotals.Salt * ratio,
                        Position = recipeComponent.Position
                    };
                }))
                .OrderBy(item => item.Position)
                .ToList();

            _context.PreparedRecipeBatches.Add(PreparedRecipeBatch);

            for (int dayOffset = 0; dayOffset < PreparedRecipeBatch.PlannedDays; dayOffset++)
            {
                DateOnly menuDate = PreparedRecipeBatch.CookedDate.AddDays(dayOffset);

                Menu? menu = await _context.Menus
                    .FirstOrDefaultAsync(menu => menu.Date == menuDate);

                if (menu is null)
                {
                    menu = new Menu
                    {
                        Name = $"Menu {menuDate}",
                        Date = menuDate
                    };

                    _context.Menus.Add(menu);
                }

                menu.MenuItems.Add(new MenuItem
                {
                    ItemType = MenuItemType.PreparedRecipeBatch,
                    PreparedRecipeBatch = PreparedRecipeBatch,
                    ServingsCount = ServingsPerDay
                });
            }

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