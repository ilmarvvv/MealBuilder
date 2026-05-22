using MealBuilder.Web.Models;

namespace MealBuilder.Web.Services
{
    public class MenuCalculationService
    {
        private readonly RecipeCalculationService _recipeCalculationService;

        public MenuCalculationService(RecipeCalculationService recipeCalculationService)
        {
            _recipeCalculationService = recipeCalculationService;
        }

        public RecipeNutritionTotals Calculate(Menu menu)
        {
            RecipeNutritionTotals totals = new();

            foreach (MenuItem menuItem in menu.MenuItems)
            {
                if (menuItem.ItemType == MenuItemType.Recipe && menuItem.Recipe is not null)
                {
                    AddRecipeItemTotals(totals, menuItem);
                }
                else if (menuItem.ItemType == MenuItemType.PreparedRecipeBatch &&
                     menuItem.PreparedRecipeBatch?.Recipe is not null &&
                     menuItem.ServingsCount is not null)
                {
                    RecipeNutritionTotals recipeTotals = _recipeCalculationService.Calculate(menuItem.PreparedRecipeBatch.Recipe);
                    RecipeNutritionTotals perServingTotals = _recipeCalculationService.Divide(recipeTotals, menuItem.PreparedRecipeBatch.Recipe.Servings);

                    totals.Calories += perServingTotals.Calories * menuItem.ServingsCount.Value;
                    totals.Protein += perServingTotals.Protein * menuItem.ServingsCount.Value;
                    totals.Fiber += perServingTotals.Fiber * menuItem.ServingsCount.Value;
                    totals.Sugar += perServingTotals.Sugar * menuItem.ServingsCount.Value;
                    totals.Salt += perServingTotals.Salt * menuItem.ServingsCount.Value;
                }
                else if (menuItem.ItemType == MenuItemType.Ingredient && menuItem.Ingredient is not null)
                {
                    AddIngredientItemTotals(totals, menuItem);
                }
            }

            return totals;
        }

        private void AddRecipeItemTotals(
            RecipeNutritionTotals totals,
            MenuItem menuItem)
        {
            if (menuItem.Recipe is null || menuItem.ServingsCount is null)
            {
                return;
            }

            RecipeNutritionTotals recipeTotals = _recipeCalculationService.Calculate(menuItem.Recipe);
            RecipeNutritionTotals perServingTotals = _recipeCalculationService.Divide(
                recipeTotals,
                menuItem.Recipe.Servings);

            totals.Calories += perServingTotals.Calories * menuItem.ServingsCount.Value;
            totals.Protein += perServingTotals.Protein * menuItem.ServingsCount.Value;
            totals.Fiber += perServingTotals.Fiber * menuItem.ServingsCount.Value;
            totals.Sugar += perServingTotals.Sugar * menuItem.ServingsCount.Value;
            totals.Salt += perServingTotals.Salt * menuItem.ServingsCount.Value;
        }

        private static void AddIngredientItemTotals(
            RecipeNutritionTotals totals,
            MenuItem menuItem)
        {
            if (menuItem.Ingredient is null || menuItem.Grams is null)
            {
                return;
            }

            totals.Calories += menuItem.Ingredient.CaloriesPer100g * menuItem.Grams.Value / 100;
            totals.Protein += menuItem.Ingredient.ProteinPer100g * menuItem.Grams.Value / 100;
            totals.Fiber += menuItem.Ingredient.FiberPer100g * menuItem.Grams.Value / 100;
            totals.Sugar += menuItem.Ingredient.SugarPer100g * menuItem.Grams.Value / 100;
            totals.Salt += menuItem.Ingredient.SaltPer100g * menuItem.Grams.Value / 100;
        }
    }
}
