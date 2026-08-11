namespace MealBuilder.Domain.Recipes;

public static class RecipeNutritionCalculator
{
    public static RecipeNutrition CalculateTotal(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var totals = RecipeNutrition.Zero;

        foreach (var recipeIngredient in recipe.Ingredients)
        {
            totals = totals.Add(
                CalculateIngredient(recipeIngredient));
        }

        return totals;
    }

    public static RecipeNutrition CalculatePerServing(Recipe recipe)
    {
        var totals = CalculateTotal(recipe);

        return totals.DivideBy(recipe.Servings);
    }

    private static RecipeNutrition CalculateIngredient(
        RecipeIngredient recipeIngredient)
    {
        var ingredient = recipeIngredient.Ingredient;
        var multiplier = recipeIngredient.Grams / 100m;

        return new RecipeNutrition(
            ingredient.CaloriesPer100g * multiplier,
            ingredient.ProteinPer100g * multiplier,
            ingredient.FatPer100g * multiplier,
            ingredient.CarbohydratesPer100g * multiplier,
            ingredient.SugarsPer100g * multiplier,
            ingredient.FiberPer100g * multiplier,
            ingredient.SaltPer100g * multiplier);
    }
}