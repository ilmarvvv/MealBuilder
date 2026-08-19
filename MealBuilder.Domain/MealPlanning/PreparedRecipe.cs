using MealBuilder.Domain.Recipes;

namespace MealBuilder.Domain.MealPlanning;

public sealed class PreparedRecipe
{
    public const int MaxNameLength = Recipe.MaxNameLength;

    private readonly List<PreparedRecipeIngredient> _ingredients =
        [];

    private PreparedRecipe()
    {
    }

    private PreparedRecipe(
        string ownerId,
        Recipe sourceRecipe,
        DateOnly preparedDate,
        decimal totalPortions)
    {
        ArgumentNullException.ThrowIfNull(sourceRecipe);

        var normalizedOwnerId = NormalizeRequired(
            ownerId,
            nameof(ownerId));

        ValidateSourceRecipe(
            sourceRecipe,
            normalizedOwnerId);

        sourceRecipe.EnsureCanBeSaved();

        OwnerId = normalizedOwnerId;
        SourceRecipeId = sourceRecipe.Id;
        SourceRecipe = sourceRecipe;
        NameSnapshot = NormalizeName(sourceRecipe.Name);
        PreparedDate = preparedDate;
        TotalPortions = ValidatePortions(totalPortions);

        foreach (var recipeIngredient in sourceRecipe.Ingredients
                     .OrderBy(recipeIngredient =>
                         recipeIngredient.Position))
        {
            _ingredients.Add(
                new PreparedRecipeIngredient(recipeIngredient));
        }
    }

    public int Id { get; private set; }

    public string OwnerId { get; private set; } =
        string.Empty;

    public int? SourceRecipeId { get; private set; }

    public Recipe? SourceRecipe { get; private set; }

    public string NameSnapshot { get; private set; } =
        string.Empty;

    public DateOnly PreparedDate { get; private set; }

    public decimal TotalPortions { get; private set; }

    public IReadOnlyCollection<PreparedRecipeIngredient>
        Ingredients => _ingredients.AsReadOnly();

    public static PreparedRecipe Create(
        string ownerId,
        Recipe sourceRecipe,
        DateOnly preparedDate,
        decimal totalPortions)
    {
        return new PreparedRecipe(
            ownerId,
            sourceRecipe,
            preparedDate,
            totalPortions);
    }

    private static void ValidateSourceRecipe(
        Recipe sourceRecipe,
        string ownerId)
    {
        if (sourceRecipe.Id <= 0)
        {
            throw new ArgumentException(
                "The source recipe must already exist.",
                nameof(sourceRecipe));
        }

        if (!string.Equals(
                sourceRecipe.OwnerId,
                ownerId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The source recipe must belong to the same owner.");
        }
    }

    private static decimal ValidatePortions(
        decimal totalPortions)
    {
        if (totalPortions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalPortions),
                totalPortions,
                "Total portions must be greater than zero.");
        }

        if (decimal.Round(totalPortions, 2) != totalPortions)
        {
            throw new ArgumentException(
                "Total portions cannot have more than two decimal places.",
                nameof(totalPortions));
        }

        return totalPortions;
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = NormalizeRequired(
            name,
            nameof(name));

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Recipe snapshot name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        return normalizedName;
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