namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeResponse(
    int Id,
    int? SourceRecipeId,
    string Name,
    DateOnly PreparedDate,
    decimal TotalPortions,
    decimal AllocatedPortions,
    decimal AvailablePortions,
    MealPlanningNutritionResponse TotalNutrition,
    MealPlanningNutritionResponse NutritionPerPortion,
    IReadOnlyList<PreparedRecipeIngredientResponse> Ingredients);