namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeSummaryResponse(
    int Id,
    string Name,
    string? Description,
    int Servings,
    int IngredientCount,
    RecipeNutritionResponse NutritionPerServing);