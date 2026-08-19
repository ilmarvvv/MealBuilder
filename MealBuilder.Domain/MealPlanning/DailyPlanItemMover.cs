namespace MealBuilder.Domain.MealPlanning;

public static class DailyPlanItemMover
{
    public static DailyPlanItem Move(
        DailyPlan sourcePlan,
        DailyPlan destinationPlan,
        int itemId,
        decimal amount)
    {
        ArgumentNullException.ThrowIfNull(sourcePlan);
        ArgumentNullException.ThrowIfNull(destinationPlan);

        EnsurePlansCanBeUsed(
            sourcePlan,
            destinationPlan);

        var sourceItem = sourcePlan.Items.SingleOrDefault(
            item => item.Id == itemId)
            ?? throw new KeyNotFoundException(
                $"Daily plan item {itemId} was not found.");

        return sourceItem.ItemType switch
        {
            DailyPlanItemType.Ingredient =>
                MoveIngredient(
                    sourcePlan,
                    destinationPlan,
                    sourceItem,
                    amount),

            DailyPlanItemType.PreparedRecipe =>
                MovePreparedRecipe(
                    sourcePlan,
                    destinationPlan,
                    sourceItem,
                    amount),

            _ => throw new InvalidOperationException(
                "The daily plan item type is not supported.")
        };
    }

    private static DailyPlanItem MoveIngredient(
        DailyPlan sourcePlan,
        DailyPlan destinationPlan,
        DailyPlanItem sourceItem,
        decimal grams)
    {
        var currentGrams = sourceItem.Grams
            ?? throw new InvalidOperationException(
                "An Ingredient item must contain grams.");

        if (grams > currentGrams)
        {
            throw new InvalidOperationException(
                "Moved grams cannot exceed the source item amount.");
        }

        var ingredient = sourceItem.Ingredient
            ?? throw new InvalidOperationException(
                "The source Ingredient must be loaded.");

        var destinationItem =
            destinationPlan.AddIngredient(
                ingredient,
                grams,
                sourceItem.PlannedTime);

        if (grams == currentGrams)
        {
            sourcePlan.RemoveItem(sourceItem.Id);
        }
        else
        {
            sourceItem.ChangeGrams(
                currentGrams - grams);
        }

        return destinationItem;
    }

    private static DailyPlanItem MovePreparedRecipe(
        DailyPlan sourcePlan,
        DailyPlan destinationPlan,
        DailyPlanItem sourceItem,
        decimal portions)
    {
        var currentPortions = sourceItem.Portions
            ?? throw new InvalidOperationException(
                "A Prepared Recipe item must contain portions.");

        if (portions > currentPortions)
        {
            throw new InvalidOperationException(
                "Moved portions cannot exceed the source item amount.");
        }

        var preparedRecipe = sourceItem.PreparedRecipe
            ?? throw new InvalidOperationException(
                "The source Prepared Recipe must be loaded.");

        var destinationItem =
            destinationPlan.AddPreparedRecipe(
                preparedRecipe,
                portions,
                sourceItem.PlannedTime);

        if (portions == currentPortions)
        {
            sourcePlan.RemoveItem(sourceItem.Id);
        }
        else
        {
            sourceItem.ChangePortions(
                currentPortions - portions);
        }

        return destinationItem;
    }

    private static void EnsurePlansCanBeUsed(
        DailyPlan sourcePlan,
        DailyPlan destinationPlan)
    {
        if (!string.Equals(
                sourcePlan.OwnerId,
                destinationPlan.OwnerId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Source and destination plans must belong to the same owner.");
        }

        if (sourcePlan.Date == destinationPlan.Date)
        {
            throw new InvalidOperationException(
                "Source and destination plans must use different dates.");
        }
    }
}