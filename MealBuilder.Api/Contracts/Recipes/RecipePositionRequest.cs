using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipePositionRequest(
    [Range(1, int.MaxValue)]
    int Position);