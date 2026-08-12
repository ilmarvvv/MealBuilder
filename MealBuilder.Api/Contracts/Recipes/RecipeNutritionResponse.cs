namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeNutritionResponse(
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal Sugars,
    decimal Fiber,
    decimal Salt);