using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record AddDailyPlanIngredientBatchRequest(
    [Range(1, int.MaxValue)]
    int IngredientId,

    [Required]
    [MinLength(1)]
    AddDailyPlanIngredientBatchEntryRequest[] Entries);