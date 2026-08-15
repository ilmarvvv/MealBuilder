namespace MealBuilder.Domain.Profiles;

public sealed record CalorieTargetEstimate(
    int Age,
    int RestingEnergyExpenditure,
    int MaintenanceCalories,
    int RecommendedDailyCalorieTarget);