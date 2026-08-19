using MealBuilder.Domain.Recipes;

namespace MealBuilder.Domain.MealPlanning;

public static class PreparedRecipeNutritionCalculator
{
    public static RecipeNutrition CalculateTotal(
        PreparedRecipe preparedRecipe)
    {
        ArgumentNullException.ThrowIfNull(preparedRecipe);

        var totals = RecipeNutrition.Zero;

        foreach (var ingredient in preparedRecipe.Ingredients)
        {
            totals = totals.Add(
                new RecipeNutrition(
                    ingredient.Calories,
                    ingredient.Protein,
                    ingredient.Fat,
                    ingredient.Carbohydrates,
                    ingredient.Sugars,
                    ingredient.Fiber,
                    ingredient.Salt));
        }

        return totals;
    }

    public static RecipeNutrition CalculatePerPortion(
        PreparedRecipe preparedRecipe)
    {
        var totals = CalculateTotal(preparedRecipe);

        return totals.DivideBy(
            preparedRecipe.TotalPortions);
    }
}