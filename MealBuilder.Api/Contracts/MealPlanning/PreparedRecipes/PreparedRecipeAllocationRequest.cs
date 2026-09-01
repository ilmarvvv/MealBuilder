using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeAllocationRequest(
    DateOnly Date,

    [Range(
        typeof(decimal),
        "0.01",
        "79228162514264337593543950335")]
    decimal Portions,

    TimeOnly? PlannedTime = null);