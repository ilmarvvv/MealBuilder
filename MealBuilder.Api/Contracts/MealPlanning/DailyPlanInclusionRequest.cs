namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record DailyPlanInclusionRequest(
    bool IncludeInWeeklySummary);