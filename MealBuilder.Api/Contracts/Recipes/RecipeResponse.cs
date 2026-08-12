namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeResponse(
    int Id,
    string Name,
    string? Description,
    int Servings,
    RecipeNutritionResponse TotalNutrition,
    RecipeNutritionResponse NutritionPerServing,
    IReadOnlyList<RecipeIngredientResponse> Ingredients,
    IReadOnlyList<RecipeStepResponse> Steps);