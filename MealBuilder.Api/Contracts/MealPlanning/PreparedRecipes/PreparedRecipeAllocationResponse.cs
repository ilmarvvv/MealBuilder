namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeAllocationResponse(
    DateOnly Date,
    decimal Portions);