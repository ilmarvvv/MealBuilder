namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeSummaryResponse(
    int Id,
    int? SourceRecipeId,
    string Name,
    DateOnly PreparedDate,
    decimal TotalPortions,
    decimal AllocatedPortions,
    decimal AvailablePortions,
    MealPlanningNutritionResponse NutritionPerPortion);