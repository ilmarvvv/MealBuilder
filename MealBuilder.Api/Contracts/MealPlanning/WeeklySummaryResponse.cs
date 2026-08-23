namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record WeeklySummaryResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    int IncludedDayCount,
    MealPlanningNutritionResponse TotalNutrition,
    MealPlanningNutritionResponse AverageNutrition,
    IReadOnlyList<WeeklyDayResponse> Days);