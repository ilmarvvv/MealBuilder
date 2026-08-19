namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeAllocationResponse(
    DateOnly Date,
    decimal Portions);