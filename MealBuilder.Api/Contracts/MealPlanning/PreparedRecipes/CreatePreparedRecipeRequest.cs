using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.MealPlanning;

namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record CreatePreparedRecipeRequest(
    [Range(1, int.MaxValue)]
    int RecipeId,

    DateOnly PreparedDate,

    [Range(
        typeof(decimal),
        "0.01",
        "79228162514264337593543950335")]
    decimal TotalPortions,

    [Required]
    [MaxLength(PreparedRecipePortionDistributor.MaxPlannedDays)]
    PreparedRecipeAllocationRequest[] Allocations);