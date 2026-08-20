using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record AddDailyPlanIngredientRequest(
    [Range(1, int.MaxValue)]
    int IngredientId,

    [Range(typeof(decimal), "0.01", "100000")]
    decimal Grams,

    TimeOnly? PlannedTime);