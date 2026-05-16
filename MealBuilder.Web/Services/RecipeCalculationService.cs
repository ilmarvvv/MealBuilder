using MealBuilder.Web.Models;

namespace MealBuilder.Web.Services
{
    public class RecipeCalculationService
    {
        public RecipeNutritionTotals Calculate(Recipe recipe)
        {
            RecipeNutritionTotals totals = new();

            foreach (RecipeIngredient recipeIngredient in recipe.RecipeIngredients)
            {
                AddIngredientTotals(totals, recipeIngredient);
            }

            foreach (RecipeComponent recipeComponent in recipe.Components)
            {
                decimal componentTotalWeight = CalculateRecipeWeight(recipeComponent.ComponentRecipe);

                if (componentTotalWeight <= 0)
                {
                    continue;
                }

                RecipeNutritionTotals componentTotals = Calculate(recipeComponent.ComponentRecipe);
                decimal ratio = recipeComponent.Grams / componentTotalWeight;

                totals.Calories += componentTotals.Calories * ratio;
                totals.Protein += componentTotals.Protein * ratio;
                totals.Fiber += componentTotals.Fiber * ratio;
                totals.Sugar += componentTotals.Sugar * ratio;
                totals.Salt += componentTotals.Salt * ratio;
            }

            return totals;
        }

        private static void AddIngredientTotals(
            RecipeNutritionTotals totals,
            RecipeIngredient recipeIngredient)
        {
            totals.Calories += recipeIngredient.Ingredient.CaloriesPer100g * recipeIngredient.Grams / 100;
            totals.Protein += recipeIngredient.Ingredient.ProteinPer100g * recipeIngredient.Grams / 100;
            totals.Fiber += recipeIngredient.Ingredient.FiberPer100g * recipeIngredient.Grams / 100;
            totals.Sugar += recipeIngredient.Ingredient.SugarPer100g * recipeIngredient.Grams / 100;
            totals.Salt += recipeIngredient.Ingredient.SaltPer100g * recipeIngredient.Grams / 100;
        }

        private static decimal CalculateRecipeWeight(Recipe recipe)
        {
            decimal totalWeight = 0;

            foreach (RecipeIngredient recipeIngredient in recipe.RecipeIngredients)
            {
                totalWeight += recipeIngredient.Grams;
            }

            foreach (RecipeComponent recipeComponent in recipe.Components)
            {
                totalWeight += recipeComponent.Grams;
            }

            return totalWeight;
        }
    }
}
