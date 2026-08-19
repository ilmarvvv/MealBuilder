namespace MealBuilder.Domain.MealPlanning;

public readonly record struct PreparedRecipeAllocationProposal(
    DateOnly Date,
    decimal Portions);