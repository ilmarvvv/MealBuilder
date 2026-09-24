using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record AddDailyPlanIngredientBatchEntryRequest(
    DateOnly Date,

    [Range(typeof(decimal), "0.01", "100000")]
    decimal Grams,

    TimeOnly? PlannedTime);