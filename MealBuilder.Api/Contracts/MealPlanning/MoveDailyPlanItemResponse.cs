namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record MoveDailyPlanItemResponse(
    DailyPlanResponse SourcePlan,
    DailyPlanResponse DestinationPlan);