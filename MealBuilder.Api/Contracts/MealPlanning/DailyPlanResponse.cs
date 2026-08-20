namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record DailyPlanResponse(
    int? Id,
    DateOnly Date,
    bool IncludeInWeeklySummary,
    MealPlanningNutritionResponse Nutrition,
    IReadOnlyList<DailyPlanItemResponse> Items);