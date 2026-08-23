namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeDeletionImpactResponse(
    int PreparedRecipeId,
    string Name,
    int AffectedItemCount,
    int AffectedDateCount);