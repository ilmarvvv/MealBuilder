namespace MealBuilder.Domain.Ingredients;

public sealed class Ingredient
{
    public const int MaxNameLength = 100;
    public const decimal MaxCaloriesPer100g = 900m;
    public const decimal MaxNutrientPer100g = 100m;

    private Ingredient()
    {
    }

    private Ingredient(
        string name,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g,
        string? ownerId,
        string? sourceName,
        string? sourceCode,
        string? sourceVersion)
    {
        Name = NormalizeName(name);

        ValidateNutrition(
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g);

        SetNutrition(
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g);

        OwnerId = ownerId;
        SourceName = sourceName;
        SourceCode = sourceCode;
        SourceVersion = sourceVersion;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal CaloriesPer100g { get; private set; }

    public decimal ProteinPer100g { get; private set; }

    public decimal FatPer100g { get; private set; }

    public decimal CarbohydratesPer100g { get; private set; }

    public decimal SugarsPer100g { get; private set; }

    public decimal FiberPer100g { get; private set; }

    public decimal SaltPer100g { get; private set; }

    public string? OwnerId { get; private set; }

    public string? SourceName { get; private set; }

    public string? SourceCode { get; private set; }

    public string? SourceVersion { get; private set; }

    public bool IsBuiltIn => OwnerId is null;

    public static Ingredient CreateUserCreated(
        string ownerId,
        string name,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g)
    {
        var normalizedOwnerId = NormalizeRequired(
            ownerId,
            nameof(ownerId));

        return new Ingredient(
            name,
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g,
            ownerId: normalizedOwnerId,
            sourceName: null,
            sourceCode: null,
            sourceVersion: null);
    }

    public static Ingredient CreateBuiltIn(
        string name,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g,
        string sourceName,
        string sourceCode,
        string sourceVersion)
    {
        var normalizedSourceName = NormalizeRequired(
            sourceName,
            nameof(sourceName));

        var normalizedSourceCode = NormalizeRequired(
            sourceCode,
            nameof(sourceCode));

        var normalizedSourceVersion = NormalizeRequired(
            sourceVersion,
            nameof(sourceVersion));

        return new Ingredient(
            name,
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g,
            ownerId: null,
            sourceName: normalizedSourceName,
            sourceCode: normalizedSourceCode,
            sourceVersion: normalizedSourceVersion);
    }

    public void UpdateUserCreated(
        string name,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g)
    {
        if (IsBuiltIn)
        {
            throw new InvalidOperationException(
                "Built-in ingredients cannot be changed.");
        }

        var normalizedName = NormalizeName(name);

        ValidateNutrition(
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g);

        Name = normalizedName;

        SetNutrition(
            caloriesPer100g,
            proteinPer100g,
            fatPer100g,
            carbohydratesPer100g,
            sugarsPer100g,
            fiberPer100g,
            saltPer100g);
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = NormalizeRequired(
            name,
            nameof(name));

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Ingredient name cannot exceed {MaxNameLength} characters.",
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

    private static void ValidateNutrition(
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g)
    {
        EnsureRange(
            caloriesPer100g,
            MaxCaloriesPer100g,
            nameof(caloriesPer100g));

        EnsureRange(
            proteinPer100g,
            MaxNutrientPer100g,
            nameof(proteinPer100g));

        EnsureRange(
            fatPer100g,
            MaxNutrientPer100g,
            nameof(fatPer100g));

        EnsureRange(
            carbohydratesPer100g,
            MaxNutrientPer100g,
            nameof(carbohydratesPer100g));

        EnsureRange(
            sugarsPer100g,
            MaxNutrientPer100g,
            nameof(sugarsPer100g));

        EnsureRange(
            fiberPer100g,
            MaxNutrientPer100g,
            nameof(fiberPer100g));

        EnsureRange(
            saltPer100g,
            MaxNutrientPer100g,
            nameof(saltPer100g));

        if (sugarsPer100g > carbohydratesPer100g)
        {
            throw new ArgumentException(
                "Sugars cannot exceed carbohydrates.",
                nameof(sugarsPer100g));
        }
    }

    private static void EnsureRange(
        decimal value,
        decimal maximum,
        string parameterName)
    {
        if (value < 0 || value > maximum)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Value must be between 0 and {maximum}.");
        }
    }

    private void SetNutrition(
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g)
    {
        CaloriesPer100g = caloriesPer100g;
        ProteinPer100g = proteinPer100g;
        FatPer100g = fatPer100g;
        CarbohydratesPer100g = carbohydratesPer100g;
        SugarsPer100g = sugarsPer100g;
        FiberPer100g = fiberPer100g;
        SaltPer100g = saltPer100g;
    }
}