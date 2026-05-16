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
                totals.Calories += recipeIngredient.Ingredient.CaloriesPer100g * recipeIngredient.Grams / 100;
                totals.Protein += recipeIngredient.Ingredient.ProteinPer100g * recipeIngredient.Grams / 100;
                totals.Fiber += recipeIngredient.Ingredient.FiberPer100g * recipeIngredient.Grams / 100;
                totals.Sugar += recipeIngredient.Ingredient.SugarPer100g * recipeIngredient.Grams / 100;
                totals.Salt += recipeIngredient.Ingredient.SaltPer100g * recipeIngredient.Grams / 100;
            }

            return totals;
        }
    }
}
