namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeDeletionImpactResponse(
    int PreparedRecipeId,
    string Name,
    int AffectedItemCount,
    int AffectedDateCount);