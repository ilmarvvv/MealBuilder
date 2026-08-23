using MealBuilder.Api.Contracts.MealPlanning.Weekly;
using MealBuilder.Domain.MealPlanning;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Mappings;

public static class WeeklySummaryResponseMapper
{
    public static WeeklySummaryResponse ToResponse(
        DateOnly startDate,
        IReadOnlyCollection<DailyPlan> dailyPlans)
    {
        ArgumentNullException.ThrowIfNull(dailyPlans);

        var endDate = startDate.AddDays(6);

        var dailyPlansByDate = dailyPlans
            .ToDictionary(dailyPlan => dailyPlan.Date);

        var days = Enumerable.Range(0, 7)
            .Select(dayOffset =>
            {
                var date = startDate.AddDays(dayOffset);

                dailyPlansByDate.TryGetValue(
                    date,
                    out var dailyPlan);

                return ToDayResponse(
                    date,
                    dailyPlan);
            })
            .ToArray();

        var includedDayCount = dailyPlans.Count(
            dailyPlan =>
                !dailyPlan.IsEmpty &&
                dailyPlan.IncludeInWeeklySummary);

        var totalNutrition =
            DailyPlanNutritionCalculator
                .CalculateWeeklyTotal(dailyPlans);

        var averageNutrition =
            DailyPlanNutritionCalculator
                .CalculateWeeklyAverage(dailyPlans);

        return new WeeklySummaryResponse(
            startDate,
            endDate,
            includedDayCount,
            DailyPlanResponseMapper.ToNutritionResponse(
                totalNutrition),
            DailyPlanResponseMapper.ToNutritionResponse(
                averageNutrition),
            days);
    }

    private static WeeklyDayResponse ToDayResponse(
        DateOnly date,
        DailyPlan? dailyPlan)
    {
        if (dailyPlan is null)
        {
            return new WeeklyDayResponse(
                date,
                null,
                false,
                false,
                DailyPlanResponseMapper.ToNutritionResponse(
                    RecipeNutrition.Zero));
        }

        var nutrition =
            DailyPlanNutritionCalculator.CalculateTotal(
                dailyPlan);

        return new WeeklyDayResponse(
            date,
            dailyPlan.Id,
            true,
            dailyPlan.IncludeInWeeklySummary,
            DailyPlanResponseMapper.ToNutritionResponse(
                nutrition));
    }
}