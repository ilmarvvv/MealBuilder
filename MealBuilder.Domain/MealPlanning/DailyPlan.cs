using MealBuilder.Domain.Ingredients;

namespace MealBuilder.Domain.MealPlanning;

public sealed class DailyPlan
{
    private readonly List<DailyPlanItem> _items = [];

    private DailyPlan()
    {
    }

    private DailyPlan(
        string ownerId,
        DateOnly date)
    {
        OwnerId = NormalizeRequired(
            ownerId,
            nameof(ownerId));

        Date = date;
        IncludeInWeeklySummary = true;
    }

    public int Id { get; private set; }

    public string OwnerId { get; private set; } =
        string.Empty;

    public DateOnly Date { get; private set; }

    public bool IncludeInWeeklySummary { get; private set; } =
        true;

    public bool IsEmpty => _items.Count == 0;

    public IReadOnlyCollection<DailyPlanItem> Items =>
        _items.AsReadOnly();

    public static DailyPlan Create(
        string ownerId,
        DateOnly date)
    {
        return new DailyPlan(ownerId, date);
    }

    public DailyPlanItem AddIngredient(
        Ingredient ingredient,
        decimal grams,
        TimeOnly? plannedTime = null)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        EnsureIngredientCanBeUsed(ingredient);

        var newItem = DailyPlanItem.CreateIngredient(
            ingredient,
            grams,
            plannedTime);

        var existingItem = _items.SingleOrDefault(item =>
            item.ItemType == DailyPlanItemType.Ingredient &&
            item.IngredientId == ingredient.Id &&
            item.PlannedTime == plannedTime);

        if (existingItem is null)
        {
            _items.Add(newItem);

            return newItem;
        }

        var existingGrams = existingItem.Grams
            ?? throw new InvalidOperationException(
                "An Ingredient item must contain grams.");

        existingItem.ChangeGrams(existingGrams + grams);

        return existingItem;
    }

    public DailyPlanItem AddPreparedRecipe(
        PreparedRecipe preparedRecipe,
        decimal portions,
        TimeOnly? plannedTime = null)
    {
        ArgumentNullException.ThrowIfNull(preparedRecipe);

        EnsurePreparedRecipeCanBeUsed(preparedRecipe);

        var newItem = DailyPlanItem.CreatePreparedRecipe(
            preparedRecipe,
            portions,
            plannedTime);

        var existingItem = _items.SingleOrDefault(item =>
            item.ItemType ==
                DailyPlanItemType.PreparedRecipe &&
            ReferencesPreparedRecipe(
                item,
                preparedRecipe) &&
            item.PlannedTime == plannedTime);

        if (existingItem is null)
        {
            _items.Add(newItem);

            return newItem;
        }

        var existingPortions = existingItem.Portions
            ?? throw new InvalidOperationException(
                "A Prepared Recipe item must contain portions.");

        existingItem.ChangePortions(
            existingPortions + portions);

        return existingItem;
    }

    public void ChangeIngredientAmount(
        int itemId,
        decimal grams)
    {
        var item = FindItem(itemId);
        item.ChangeGrams(grams);
    }

    public void ChangePreparedRecipeAmount(
        int itemId,
        decimal portions)
    {
        var item = FindItem(itemId);
        item.ChangePortions(portions);
    }

    public DailyPlanItem ChangePlannedTime(
        int itemId,
        TimeOnly? plannedTime)
    {
        var item = FindItem(itemId);

        if (item.PlannedTime == plannedTime)
        {
            return item;
        }

        var matchingItem = _items.SingleOrDefault(candidate =>
            !ReferenceEquals(candidate, item) &&
            candidate.PlannedTime == plannedTime &&
            HasSameFoodSource(candidate, item));

        if (matchingItem is null)
        {
            item.ChangePlannedTime(plannedTime);

            return item;
        }

        switch (item.ItemType)
        {
            case DailyPlanItemType.Ingredient:
                {
                    var matchingGrams = matchingItem.Grams
                        ?? throw new InvalidOperationException(
                            "An Ingredient item must contain grams.");

                    var itemGrams = item.Grams
                        ?? throw new InvalidOperationException(
                            "An Ingredient item must contain grams.");

                    matchingItem.ChangeGrams(
                        matchingGrams + itemGrams);

                    break;
                }

            case DailyPlanItemType.PreparedRecipe:
                {
                    var matchingPortions = matchingItem.Portions
                        ?? throw new InvalidOperationException(
                            "A Prepared Recipe item must contain portions.");

                    var itemPortions = item.Portions
                        ?? throw new InvalidOperationException(
                            "A Prepared Recipe item must contain portions.");

                    matchingItem.ChangePortions(
                        matchingPortions + itemPortions);

                    break;
                }

            default:
                throw new InvalidOperationException(
                    "The daily plan item type is not supported.");
        }

        _items.Remove(item);

        return matchingItem;
    }

    public DailyPlanItem RemoveItem(int itemId)
    {
        var item = FindItem(itemId);

        _items.Remove(item);

        return item;
    }

    public void SetWeeklySummaryInclusion(
        bool includeInWeeklySummary)
    {
        IncludeInWeeklySummary = includeInWeeklySummary;
    }

    public void EnsureCanBeSaved()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(
                "A daily plan must contain at least one item.");
        }
    }

    private static bool ReferencesPreparedRecipe(
        DailyPlanItem item,
        PreparedRecipe preparedRecipe)
    {
        if (preparedRecipe.Id > 0)
        {
            return item.PreparedRecipeId ==
                preparedRecipe.Id;
        }

        return ReferenceEquals(
            item.PreparedRecipe,
            preparedRecipe);
    }

    private static bool HasSameFoodSource(
        DailyPlanItem firstItem,
        DailyPlanItem secondItem)
    {
        if (firstItem.ItemType != secondItem.ItemType)
        {
            return false;
        }

        return firstItem.ItemType switch
        {
            DailyPlanItemType.Ingredient =>
                firstItem.IngredientId ==
                secondItem.IngredientId,

            DailyPlanItemType.PreparedRecipe =>
                ReferencesSamePreparedRecipe(
                    firstItem,
                    secondItem),

            _ => false
        };
    }

    private static bool ReferencesSamePreparedRecipe(
        DailyPlanItem firstItem,
        DailyPlanItem secondItem)
    {
        if (firstItem.PreparedRecipeId.HasValue ||
            secondItem.PreparedRecipeId.HasValue)
        {
            return firstItem.PreparedRecipeId ==
                secondItem.PreparedRecipeId;
        }

        return ReferenceEquals(
            firstItem.PreparedRecipe,
            secondItem.PreparedRecipe);
    }

    private DailyPlanItem FindItem(int itemId)
    {
        return _items.SingleOrDefault(item => item.Id == itemId)
            ?? throw new KeyNotFoundException(
                $"Daily plan item {itemId} was not found.");
    }

    private void EnsureIngredientCanBeUsed(
        Ingredient ingredient)
    {
        if (ingredient.OwnerId is not null &&
            !string.Equals(
                ingredient.OwnerId,
                OwnerId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The ingredient must be built-in or belong to the daily plan owner.");
        }
    }

    private void EnsurePreparedRecipeCanBeUsed(
        PreparedRecipe preparedRecipe)
    {
        if (!string.Equals(
                preparedRecipe.OwnerId,
                OwnerId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The Prepared Recipe must belong to the daily plan owner.");
        }

        if (Date < preparedRecipe.PreparedDate)
        {
            throw new InvalidOperationException(
                "Prepared Recipe portions cannot be allocated before the prepared date.");
        }
    }

    private static string NormalizeRequired(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }
}