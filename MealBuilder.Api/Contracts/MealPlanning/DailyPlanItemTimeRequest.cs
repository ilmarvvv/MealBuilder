namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record DailyPlanItemTimeRequest(
    TimeOnly? PlannedTime);