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
                else if (menuItem.ItemType == MenuItemType.PreparedRecipeBatch)
                {
                    AddPreparedRecipeBatchItemTotals(totals, menuItem);
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
                menuItem.Recipe.TotalServings);

            totals.Calories += perServingTotals.Calories * menuItem.ServingsCount.Value;
            totals.Protein += perServingTotals.Protein * menuItem.ServingsCount.Value;
            totals.Fiber += perServingTotals.Fiber * menuItem.ServingsCount.Value;
            totals.Sugar += perServingTotals.Sugar * menuItem.ServingsCount.Value;
            totals.Salt += perServingTotals.Salt * menuItem.ServingsCount.Value;
        }

        private static void AddPreparedRecipeBatchItemTotals(
            RecipeNutritionTotals totals,
            MenuItem menuItem)
        {
            if (menuItem.PreparedRecipeBatch is null || menuItem.ServingsCount is null)
            {
                return;
            }

            if (menuItem.PreparedRecipeBatch.TotalServings <= 0)
            {
                return;
            }

            decimal servingRatio = menuItem.ServingsCount.Value / menuItem.PreparedRecipeBatch.TotalServings;

            decimal totalCalories = menuItem.PreparedRecipeBatch.Items.Sum(item => item.CaloriesSnapshot);
            decimal totalProtein = menuItem.PreparedRecipeBatch.Items.Sum(item => item.ProteinSnapshot);
            decimal totalFiber = menuItem.PreparedRecipeBatch.Items.Sum(item => item.FiberSnapshot);
            decimal totalSugar = menuItem.PreparedRecipeBatch.Items.Sum(item => item.SugarSnapshot);
            decimal totalSalt = menuItem.PreparedRecipeBatch.Items.Sum(item => item.SaltSnapshot);

            totals.Calories += totalCalories * servingRatio;
            totals.Protein += totalProtein * servingRatio;
            totals.Fiber += totalFiber * servingRatio;
            totals.Sugar += totalSugar * servingRatio;
            totals.Salt += totalSalt * servingRatio;
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
