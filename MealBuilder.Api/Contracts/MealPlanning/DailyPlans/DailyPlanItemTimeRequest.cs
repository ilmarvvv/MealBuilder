namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record DailyPlanItemTimeRequest(
    TimeOnly? PlannedTime);