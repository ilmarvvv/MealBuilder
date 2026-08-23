namespace MealBuilder.Api.Contracts.MealPlanning.Weekly;

public sealed record WeeklyDayResponse(
    DateOnly Date,
    int? DailyPlanId,
    bool HasPlan,
    bool IncludeInWeeklySummary,
    MealPlanningNutritionResponse Nutrition);