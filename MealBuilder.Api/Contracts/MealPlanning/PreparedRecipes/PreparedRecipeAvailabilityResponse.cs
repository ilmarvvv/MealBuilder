namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeAvailabilityResponse(
    int PreparedRecipeId,
    decimal TotalPortions,
    decimal AllocatedPortions,
    decimal AvailablePortions);