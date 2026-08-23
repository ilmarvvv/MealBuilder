using MealBuilder.Api.Contracts.MealPlanning;
using MealBuilder.Domain.MealPlanning;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Mappings;

public static class DailyPlanResponseMapper
{
    public static DailyPlanResponse ToResponse(
        DailyPlan dailyPlan)
    {
        var nutrition =
            DailyPlanNutritionCalculator.CalculateTotal(
                dailyPlan);

        var items = dailyPlan.Items
            .OrderBy(item => item.PlannedTime is null)
            .ThenBy(item => item.PlannedTime)
            .ThenBy(item => item.Id)
            .Select(ToItemResponse)
            .ToArray();

        return new DailyPlanResponse(
            dailyPlan.Id,
            dailyPlan.Date,
            dailyPlan.IncludeInWeeklySummary,
            ToNutritionResponse(nutrition),
            items);
    }

    public static DailyPlanResponse ToEmptyResponse(
        DateOnly date)
    {
        return new DailyPlanResponse(
            null,
            date,
            true,
            ToNutritionResponse(RecipeNutrition.Zero),
            Array.Empty<DailyPlanItemResponse>());
    }

    private static DailyPlanItemResponse ToItemResponse(
        DailyPlanItem item)
    {
        var nutrition =
            DailyPlanNutritionCalculator.CalculateItem(
                item);

        return new DailyPlanItemResponse(
            item.Id,
            item.ItemType,
            item.IngredientId,
            item.PreparedRecipeId,
            GetItemName(item),
            item.Grams,
            item.Portions,
            item.PlannedTime,
            ToNutritionResponse(nutrition));
    }

    private static string GetItemName(
        DailyPlanItem item)
    {
        return item.ItemType switch
        {
            DailyPlanItemType.Ingredient =>
                item.Ingredient?.Name
                ?? throw new InvalidOperationException(
                    "The Daily Plan Ingredient must be loaded."),

            DailyPlanItemType.PreparedRecipe =>
                item.PreparedRecipe?.NameSnapshot
                ?? throw new InvalidOperationException(
                    "The Daily Plan Prepared Recipe must be loaded."),

            _ => throw new InvalidOperationException(
                "The Daily Plan item type is not supported.")
        };
    }

    internal static MealPlanningNutritionResponse
        ToNutritionResponse(
            RecipeNutrition nutrition)
    {
        return new MealPlanningNutritionResponse(
            Round(nutrition.Calories),
            Round(nutrition.Protein),
            Round(nutrition.Fat),
            Round(nutrition.Carbohydrates),
            Round(nutrition.Sugars),
            Round(nutrition.Fiber),
            Round(nutrition.Salt));
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}