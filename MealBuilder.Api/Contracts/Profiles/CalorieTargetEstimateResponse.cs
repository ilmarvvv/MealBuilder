namespace MealBuilder.Api.Contracts.Profiles;

public sealed record CalorieTargetEstimateResponse(
    int Age,
    int RestingEnergyExpenditure,
    int MaintenanceCalories,
    int RecommendedDailyCalorieTarget);