using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.Profiles;

namespace MealBuilder.Api.Contracts.Profiles;

public sealed record CalculatedProfileRequest(
    [Range(
        UserNutritionProfile.MinDailyCalorieTarget,
        UserNutritionProfile.MaxDailyCalorieTarget)]
    int DailyCalorieTarget,

    [Required]
    CalorieTargetCalculationRequest? CalculationInputs);