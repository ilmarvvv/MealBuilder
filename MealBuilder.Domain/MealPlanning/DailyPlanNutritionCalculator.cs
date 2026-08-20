using MealBuilder.Domain.Recipes;

namespace MealBuilder.Domain.MealPlanning;

public static class DailyPlanNutritionCalculator
{
    public static RecipeNutrition CalculateTotal(
        DailyPlan dailyPlan)
    {
        ArgumentNullException.ThrowIfNull(dailyPlan);

        var totals = RecipeNutrition.Zero;

        foreach (var item in dailyPlan.Items)
        {
            totals = totals.Add(
                CalculateItem(item));
        }

        return totals;
    }

    public static RecipeNutrition CalculateWeeklyTotal(
        IEnumerable<DailyPlan> dailyPlans)
    {
        var includedPlans = GetIncludedPlans(dailyPlans);
        var totals = RecipeNutrition.Zero;

        foreach (var dailyPlan in includedPlans)
        {
            totals = totals.Add(
                CalculateTotal(dailyPlan));
        }

        return totals;
    }

    public static RecipeNutrition CalculateWeeklyAverage(
        IEnumerable<DailyPlan> dailyPlans)
    {
        var includedPlans = GetIncludedPlans(dailyPlans);

        if (includedPlans.Count == 0)
        {
            return RecipeNutrition.Zero;
        }

        var totals = RecipeNutrition.Zero;

        foreach (var dailyPlan in includedPlans)
        {
            totals = totals.Add(
                CalculateTotal(dailyPlan));
        }

        return totals.DivideBy(includedPlans.Count);
    }

    private static List<DailyPlan> GetIncludedPlans(
        IEnumerable<DailyPlan> dailyPlans)
    {
        ArgumentNullException.ThrowIfNull(dailyPlans);

        return dailyPlans
            .Where(dailyPlan =>
                !dailyPlan.IsEmpty &&
                dailyPlan.IncludeInWeeklySummary)
            .ToList();
    }

    public static RecipeNutrition CalculateItem(
        DailyPlanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.ItemType switch
        {
            DailyPlanItemType.Ingredient =>
                CalculateIngredient(item),

            DailyPlanItemType.PreparedRecipe =>
                CalculatePreparedRecipe(item),

            _ => throw new InvalidOperationException(
                "The daily plan item type is not supported.")
        };
    }

    private static RecipeNutrition CalculateIngredient(
        DailyPlanItem item)
    {
        var ingredient = item.Ingredient
            ?? throw new InvalidOperationException(
                "The Daily Plan Ingredient must be loaded.");

        var grams = item.Grams
            ?? throw new InvalidOperationException(
                "An Ingredient item must contain grams.");

        var multiplier = grams / 100m;

        return new RecipeNutrition(
            ingredient.CaloriesPer100g * multiplier,
            ingredient.ProteinPer100g * multiplier,
            ingredient.FatPer100g * multiplier,
            ingredient.CarbohydratesPer100g * multiplier,
            ingredient.SugarsPer100g * multiplier,
            ingredient.FiberPer100g * multiplier,
            ingredient.SaltPer100g * multiplier);
    }

    private static RecipeNutrition CalculatePreparedRecipe(
        DailyPlanItem item)
    {
        var preparedRecipe = item.PreparedRecipe
            ?? throw new InvalidOperationException(
                "The Daily Plan Prepared Recipe must be loaded.");

        var portions = item.Portions
            ?? throw new InvalidOperationException(
                "A Prepared Recipe item must contain portions.");

        var perPortion =
            PreparedRecipeNutritionCalculator
                .CalculatePerPortion(preparedRecipe);

        return perPortion.MultiplyBy(portions);
    }
}