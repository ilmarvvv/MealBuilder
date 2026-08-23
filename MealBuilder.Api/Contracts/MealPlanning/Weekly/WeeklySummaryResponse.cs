namespace MealBuilder.Api.Contracts.MealPlanning.Weekly;

public sealed record WeeklySummaryResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    int IncludedDayCount,
    MealPlanningNutritionResponse TotalNutrition,
    MealPlanningNutritionResponse AverageNutrition,
    IReadOnlyList<WeeklyDayResponse> Days);