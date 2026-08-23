namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record DailyPlanInclusionRequest(
    bool IncludeInWeeklySummary);