namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record WeeklyDayResponse(
    DateOnly Date,
    int? DailyPlanId,
    bool HasPlan,
    bool IncludeInWeeklySummary,
    MealPlanningNutritionResponse Nutrition);