using MealBuilder.Domain.Ingredients;

namespace MealBuilder.Domain.MealPlanning;

public sealed class DailyPlanItem
{
    public const decimal MaxGrams = 100000m;

    private DailyPlanItem()
    {
    }

    private DailyPlanItem(
        Ingredient ingredient,
        decimal grams,
        TimeOnly? plannedTime)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (ingredient.Id <= 0)
        {
            throw new ArgumentException(
                "The ingredient must already exist.",
                nameof(ingredient));
        }

        ItemType = DailyPlanItemType.Ingredient;
        IngredientId = ingredient.Id;
        Ingredient = ingredient;
        Grams = ValidateGrams(grams);
        PlannedTime = plannedTime;
    }

    private DailyPlanItem(
        PreparedRecipe preparedRecipe,
        decimal portions,
        TimeOnly? plannedTime)
    {
        ArgumentNullException.ThrowIfNull(preparedRecipe);

        ItemType = DailyPlanItemType.PreparedRecipe;
        PreparedRecipeId = preparedRecipe.Id > 0
            ? preparedRecipe.Id
            : null;
        PreparedRecipe = preparedRecipe;
        Portions = ValidatePortions(portions);
        PlannedTime = plannedTime;
    }

    public int Id { get; private set; }

    public int DailyPlanId { get; private set; }

    public DailyPlanItemType ItemType { get; private set; }

    public int? IngredientId { get; private set; }

    public Ingredient? Ingredient { get; private set; }

    public int? PreparedRecipeId { get; private set; }

    public PreparedRecipe? PreparedRecipe { get; private set; }

    public decimal? Grams { get; private set; }

    public decimal? Portions { get; private set; }

    public TimeOnly? PlannedTime { get; private set; }

    internal static DailyPlanItem CreateIngredient(
        Ingredient ingredient,
        decimal grams,
        TimeOnly? plannedTime)
    {
        return new DailyPlanItem(
            ingredient,
            grams,
            plannedTime);
    }

    internal static DailyPlanItem CreatePreparedRecipe(
        PreparedRecipe preparedRecipe,
        decimal portions,
        TimeOnly? plannedTime)
    {
        return new DailyPlanItem(
            preparedRecipe,
            portions,
            plannedTime);
    }

    internal void ChangeGrams(decimal grams)
    {
        if (ItemType != DailyPlanItemType.Ingredient)
        {
            throw new InvalidOperationException(
                "Only an Ingredient item uses grams.");
        }

        Grams = ValidateGrams(grams);
    }

    internal void ChangePortions(decimal portions)
    {
        if (ItemType != DailyPlanItemType.PreparedRecipe)
        {
            throw new InvalidOperationException(
                "Only a Prepared Recipe item uses portions.");
        }

        Portions = ValidatePortions(portions);
    }

    internal void ChangePlannedTime(TimeOnly? plannedTime)
    {
        PlannedTime = plannedTime;
    }

    private static decimal ValidateGrams(decimal grams)
    {
        if (grams <= 0 || grams > MaxGrams)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grams),
                grams,
                $"Grams must be greater than zero and cannot exceed {MaxGrams}.");
        }

        return grams;
    }

    private static decimal ValidatePortions(decimal portions)
    {
        if (portions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(portions),
                portions,
                "Portions must be greater than zero.");
        }

        if (decimal.Round(portions, 2) != portions)
        {
            throw new ArgumentException(
                "Portions cannot have more than two decimal places.",
                nameof(portions));
        }

        return portions;
    }
}