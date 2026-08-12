namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeIngredientResponse(
    int IngredientId,
    string IngredientName,
    decimal Grams,
    int Position);