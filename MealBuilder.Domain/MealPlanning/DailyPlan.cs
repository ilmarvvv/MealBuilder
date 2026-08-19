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

    public void ChangePlannedTime(
        int itemId,
        TimeOnly? plannedTime)
    {
        var item = FindItem(itemId);
        item.ChangePlannedTime(plannedTime);
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