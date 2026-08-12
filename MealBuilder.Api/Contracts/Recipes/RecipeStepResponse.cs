namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeStepResponse(
    int Id,
    string Instruction,
    int Position);