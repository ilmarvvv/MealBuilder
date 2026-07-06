using MealBuilder.Web.Data;
using MealBuilder.Web.Models;
using MealBuilder.Web.Services;
using MealBuilder.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace MealBuilder.Web.Pages.Recipes
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RecipeCalculationService _recipeCalculationService;

        public EditModel(AppDbContext context, RecipeCalculationService recipeCalculationService)
        {
            _context = context;
            _recipeCalculationService = recipeCalculationService;
        }

        [BindProperty]
        public Recipe Recipe { get; set; } = new();

        public List<RecipeContentSummary> RecipeContentSummaries { get; set; } = [];

        public RecipeNutritionTotals Totals { get; set; } = new();

        public RecipeNutritionTotals PerDayTotals { get; set; } = new();

        public RecipeNutritionTotals PerServingTotals { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Recipe? recipe = await LoadRecipeAsync(id);

            if (recipe is null)
            {
                return NotFound();
            }

            Recipe = recipe;
            LoadRecipeViewData(recipe);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Recipe? recipe = await LoadRecipeAsync(Recipe.Id);

                if (recipe is not null)
                {
                    LoadRecipeViewData(recipe);
                }

                return Page();
            }

            _context.Recipes.Update(Recipe);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = Recipe.Id });
        }

        public async Task<IActionResult> OnPostRemoveIngredientAsync(int recipeIngredientId)
        {
            RecipeIngredient? recipeIngredient = await _context.RecipeIngredients
                .FindAsync(recipeIngredientId);

            if (recipeIngredient is null)
            {
                return NotFound();
            }

            int recipeId = recipeIngredient.RecipeId;

            _context.RecipeIngredients.Remove(recipeIngredient);
            await _context.SaveChangesAsync();

            await ReorderRecipeContentsAsync(recipeId);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = recipeId });
        }

        public async Task<IActionResult> OnPostRemoveComponentAsync(int recipeComponentId)
        {
            RecipeComponent? recipeComponent = await _context.RecipeComponents
                .FindAsync(recipeComponentId);

            if (recipeComponent is null)
            {
                return NotFound();
            }

            int recipeId = recipeComponent.ParentRecipeId;

            _context.RecipeComponents.Remove(recipeComponent);
            await _context.SaveChangesAsync();

            await ReorderRecipeContentsAsync(recipeId);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = recipeId });
        }

        public async Task<IActionResult> OnPostChangeContentPositionAsync(
            string contentType,
            int contentId,
            int newPosition)
        {
            RecipeIngredient? recipeIngredient = null;
            RecipeComponent? recipeComponent = null;
            int recipeId;

            if (contentType == "Ingredient")
            {
                recipeIngredient = await _context.RecipeIngredients.FindAsync(contentId);

                if (recipeIngredient is null)
                {
                    return NotFound();
                }

                recipeId = recipeIngredient.RecipeId;
            }
            else
            {
                recipeComponent = await _context.RecipeComponents.FindAsync(contentId);

                if (recipeComponent is null)
                {
                    return NotFound();
                }

                recipeId = recipeComponent.ParentRecipeId;
            }

            List<RecipeContentPositionItem> contentItems = await LoadRecipeContentPositionItemsAsync(recipeId);

            RecipeContentPositionItem? movedItem = contentItems
                .FirstOrDefault(contentItem =>
                    contentItem.ContentType == contentType &&
                    contentItem.Id == contentId);

            if (movedItem is null)
            {
                return NotFound();
            }

            contentItems.Remove(movedItem);

            int insertIndex = Math.Clamp(newPosition - 1, 0, contentItems.Count);
            contentItems.Insert(insertIndex, movedItem);

            await ApplyRecipeContentPositionsAsync(contentItems);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = recipeId });
        }

        private async Task<List<RecipeContentPositionItem>> LoadRecipeContentPositionItemsAsync(int recipeId)
        {
            List<RecipeContentPositionItem> ingredientItems = await _context.RecipeIngredients
                .Where(recipeIngredient => recipeIngredient.RecipeId == recipeId)
                .Select(recipeIngredient => new RecipeContentPositionItem
                {
                    Id = recipeIngredient.Id,
                    ContentType = "Ingredient",
                    Position = recipeIngredient.Position
                })
                .ToListAsync();

            List<RecipeContentPositionItem> componentItems = await _context.RecipeComponents
                .Where(recipeComponent => recipeComponent.ParentRecipeId == recipeId)
                .Select(recipeComponent => new RecipeContentPositionItem
                {
                    Id = recipeComponent.Id,
                    ContentType = "Recipe",
                    Position = recipeComponent.Position
                })
                .ToListAsync();

            return ingredientItems
                .Concat(componentItems)
                .OrderBy(contentItem => contentItem.Position)
                .ThenBy(contentItem => contentItem.ContentType)
                .ThenBy(contentItem => contentItem.Id)
                .ToList();
        }

        private async Task ApplyRecipeContentPositionsAsync(List<RecipeContentPositionItem> contentItems)
        {
            for (int index = 0; index < contentItems.Count; index++)
            {
                RecipeContentPositionItem contentItem = contentItems[index];
                int position = index + 1;

                if (contentItem.ContentType == "Ingredient")
                {
                    RecipeIngredient? recipeIngredient = await _context.RecipeIngredients.FindAsync(contentItem.Id);

                    if (recipeIngredient is not null)
                    {
                        recipeIngredient.Position = position;
                    }
                }
                else
                {
                    RecipeComponent? recipeComponent = await _context.RecipeComponents.FindAsync(contentItem.Id);

                    if (recipeComponent is not null)
                    {
                        recipeComponent.Position = position;
                    }
                }
            }
        }

        private class RecipeContentPositionItem
        {
            public int Id { get; set; }

            public string ContentType { get; set; } = string.Empty;

            public int Position { get; set; }
        }

        private async Task ReorderRecipeContentsAsync(int recipeId)
        {
            List<RecipeIngredient> recipeIngredients = await _context.RecipeIngredients
                .Where(recipeIngredient => recipeIngredient.RecipeId == recipeId)
                .OrderBy(recipeIngredient => recipeIngredient.Position)
                .ThenBy(recipeIngredient => recipeIngredient.Id)
                .ToListAsync();

            List<RecipeComponent> recipeComponents = await _context.RecipeComponents
                .Where(recipeComponent => recipeComponent.ParentRecipeId == recipeId)
                .OrderBy(recipeComponent => recipeComponent.Position)
                .ThenBy(recipeComponent => recipeComponent.Id)
                .ToListAsync();

            int position = 1;

            foreach (RecipeIngredient recipeIngredient in recipeIngredients)
            {
                recipeIngredient.Position = position;
                position++;
            }

            foreach (RecipeComponent recipeComponent in recipeComponents)
            {
                recipeComponent.Position = position;
                position++;
            }
        }

        private async Task<Recipe?> LoadRecipeAsync(int id)
        {
            return await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .Include(recipe => recipe.Components)
                .ThenInclude(recipeComponent => recipeComponent.ComponentRecipe)
                .ThenInclude(componentRecipe => componentRecipe.RecipeIngredients)
                .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
                .FirstOrDefaultAsync(recipe => recipe.Id == id);
        }

        private void LoadRecipeViewData(Recipe recipe)
        {
            RecipeContentSummaries = BuildRecipeContentSummaries(recipe);
            Totals = _recipeCalculationService.Calculate(recipe);
            PerDayTotals = _recipeCalculationService.Divide(Totals, recipe.DefaultPlannedDays);
            PerServingTotals = _recipeCalculationService.Divide(Totals, recipe.TotalServings);
        }

        private List<RecipeContentSummary> BuildRecipeContentSummaries(Recipe recipe)
        {
            return recipe.RecipeIngredients
                .Select(recipeIngredient => new RecipeContentSummary
                {
                    Id = recipeIngredient.Id,
                    Position = recipeIngredient.Position,
                    Type = "Ingredient",
                    Name = recipeIngredient.Ingredient.Name,
                    Grams = recipeIngredient.Grams,
                    Calories = recipeIngredient.Ingredient.CaloriesPer100g * recipeIngredient.Grams / 100,
                    Protein = recipeIngredient.Ingredient.ProteinPer100g * recipeIngredient.Grams / 100,
                    Fiber = recipeIngredient.Ingredient.FiberPer100g * recipeIngredient.Grams / 100,
                    Sugar = recipeIngredient.Ingredient.SugarPer100g * recipeIngredient.Grams / 100,
                    Salt = recipeIngredient.Ingredient.SaltPer100g * recipeIngredient.Grams / 100
                })
                .Concat(recipe.Components.Select(recipeComponent =>
                {
                    decimal componentTotalWeight = _recipeCalculationService.CalculateEffectiveWeight(recipeComponent.ComponentRecipe);
                    RecipeNutritionTotals componentTotals = _recipeCalculationService.Calculate(recipeComponent.ComponentRecipe);
                    decimal ratio = componentTotalWeight > 0 ? recipeComponent.Grams / componentTotalWeight : 0;

                    return new RecipeContentSummary
                    {
                        Id = recipeComponent.Id,
                        Position = recipeComponent.Position,
                        Type = "Recipe",
                        Name = recipeComponent.ComponentRecipe.Name,
                        Grams = recipeComponent.Grams,
                        Calories = componentTotals.Calories * ratio,
                        Protein = componentTotals.Protein * ratio,
                        Fiber = componentTotals.Fiber * ratio,
                        Sugar = componentTotals.Sugar * ratio,
                        Salt = componentTotals.Salt * ratio
                    };
                }))
                .OrderBy(recipeContent => recipeContent.Position)
                .ToList();
        }
    }
}