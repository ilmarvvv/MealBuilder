namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeAvailabilityResponse(
    int PreparedRecipeId,
    decimal TotalPortions,
    decimal AllocatedPortions,
    decimal AvailablePortions);