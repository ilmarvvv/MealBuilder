namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record MoveDailyPlanItemResponse(
    DailyPlanResponse SourcePlan,
    DailyPlanResponse DestinationPlan);