using MealBuilder.Domain.Profiles;

namespace MealBuilder.Api.Contracts.Profiles;

public sealed record UserNutritionProfileResponse(
    int DailyCalorieTarget,
    DateOnly? BirthDate,
    CalculationSex? SexForCalculation,
    decimal? HeightCm,
    decimal? WeightKg,
    ActivityLevel? ActivityLevel,
    WeightGoal? WeightGoal,
    bool HasCalculationInputs);