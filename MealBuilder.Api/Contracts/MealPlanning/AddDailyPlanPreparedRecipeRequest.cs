using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record AddDailyPlanPreparedRecipeRequest(
    [Range(1, int.MaxValue)]
    int PreparedRecipeId,

    [Range(
        typeof(decimal),
        "0.01",
        "79228162514264337593543950335")]
    decimal Portions,

    TimeOnly? PlannedTime);