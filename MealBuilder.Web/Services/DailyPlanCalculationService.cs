using MealBuilder.Web.Models;

namespace MealBuilder.Web.Services
{
    public class DailyPlanCalculationService
    {
        private readonly RecipeCalculationService _recipeCalculationService;

        public DailyPlanCalculationService(RecipeCalculationService recipeCalculationService)
        {
            _recipeCalculationService = recipeCalculationService;
        }

        public RecipeNutritionTotals Calculate(DailyPlan dailyPlan)
        {
            RecipeNutritionTotals totals = new();

            foreach (DailyPlanItem dailyPlanItem in dailyPlan.DailyPlanItems)
            {
                if (dailyPlanItem.ItemType == DailyPlanItemType.Recipe && dailyPlanItem.Recipe is not null)
                {
                    AddRecipeItemTotals(totals, dailyPlanItem);
                }
                else if (dailyPlanItem.ItemType == DailyPlanItemType.PreparedRecipeBatch)
                {
                    AddPreparedRecipeBatchItemTotals(totals, dailyPlanItem);
                }
                else if (dailyPlanItem.ItemType == DailyPlanItemType.Ingredient && dailyPlanItem.Ingredient is not null)
                {
                    AddIngredientItemTotals(totals, dailyPlanItem);
                }
            }

            return totals;
        }

        private void AddRecipeItemTotals(
            RecipeNutritionTotals totals,
            DailyPlanItem dailyPlanItem)
        {
            if (dailyPlanItem.Recipe is null || dailyPlanItem.ServingsCount is null)
            {
                return;
            }

            RecipeNutritionTotals recipeTotals = _recipeCalculationService.Calculate(dailyPlanItem.Recipe);
            RecipeNutritionTotals perServingTotals = _recipeCalculationService.Divide(
                recipeTotals,
                dailyPlanItem.Recipe.TotalServings);

            totals.Calories += perServingTotals.Calories * dailyPlanItem.ServingsCount.Value;
            totals.Protein += perServingTotals.Protein * dailyPlanItem.ServingsCount.Value;
            totals.Fiber += perServingTotals.Fiber * dailyPlanItem.ServingsCount.Value;
            totals.Sugar += perServingTotals.Sugar * dailyPlanItem.ServingsCount.Value;
            totals.Salt += perServingTotals.Salt * dailyPlanItem.ServingsCount.Value;
        }

        private static void AddPreparedRecipeBatchItemTotals(
            RecipeNutritionTotals totals,
            DailyPlanItem dailyPlanItem)
        {
            if (dailyPlanItem.PreparedRecipeBatch is null || dailyPlanItem.ServingsCount is null)
            {
                return;
            }

            if (dailyPlanItem.PreparedRecipeBatch.TotalServings <= 0)
            {
                return;
            }

            decimal servingRatio = dailyPlanItem.ServingsCount.Value / dailyPlanItem.PreparedRecipeBatch.TotalServings;

            decimal totalCalories = dailyPlanItem.PreparedRecipeBatch.Items.Sum(item => item.CaloriesSnapshot);
            decimal totalProtein = dailyPlanItem.PreparedRecipeBatch.Items.Sum(item => item.ProteinSnapshot);
            decimal totalFiber = dailyPlanItem.PreparedRecipeBatch.Items.Sum(item => item.FiberSnapshot);
            decimal totalSugar = dailyPlanItem.PreparedRecipeBatch.Items.Sum(item => item.SugarSnapshot);
            decimal totalSalt = dailyPlanItem.PreparedRecipeBatch.Items.Sum(item => item.SaltSnapshot);

            totals.Calories += totalCalories * servingRatio;
            totals.Protein += totalProtein * servingRatio;
            totals.Fiber += totalFiber * servingRatio;
            totals.Sugar += totalSugar * servingRatio;
            totals.Salt += totalSalt * servingRatio;
        }

        private static void AddIngredientItemTotals(
            RecipeNutritionTotals totals,
            DailyPlanItem dailyPlanItem)
        {
            if (dailyPlanItem.Ingredient is null || dailyPlanItem.Grams is null)
            {
                return;
            }

            totals.Calories += dailyPlanItem.Ingredient.CaloriesPer100g * dailyPlanItem.Grams.Value / 100;
            totals.Protein += dailyPlanItem.Ingredient.ProteinPer100g * dailyPlanItem.Grams.Value / 100;
            totals.Fiber += dailyPlanItem.Ingredient.FiberPer100g * dailyPlanItem.Grams.Value / 100;
            totals.Sugar += dailyPlanItem.Ingredient.SugarPer100g * dailyPlanItem.Grams.Value / 100;
            totals.Salt += dailyPlanItem.Ingredient.SaltPer100g * dailyPlanItem.Grams.Value / 100;
        }
    }
}
