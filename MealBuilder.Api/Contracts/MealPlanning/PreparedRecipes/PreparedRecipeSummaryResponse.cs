namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeSummaryResponse(
    int Id,
    int? SourceRecipeId,
    string Name,
    DateOnly PreparedDate,
    decimal TotalPortions,
    decimal AllocatedPortions,
    decimal AvailablePortions,
    MealPlanningNutritionResponse NutritionPerPortion);