using MealBuilder.Domain.Ingredients;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Domain.MealPlanning;

public sealed class PreparedRecipeIngredient
{
    public const int MaxNameLength = Ingredient.MaxNameLength;

    private PreparedRecipeIngredient()
    {
    }

    internal PreparedRecipeIngredient(
        RecipeIngredient recipeIngredient)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredient);

        if (recipeIngredient.Ingredient is null)
        {
            throw new InvalidOperationException(
                "The source recipe ingredient must include its ingredient.");
        }

        var ingredient = recipeIngredient.Ingredient;
        var multiplier = recipeIngredient.Grams / 100m;

        NameSnapshot = NormalizeName(ingredient.Name);
        Grams = recipeIngredient.Grams;
        Position = recipeIngredient.Position;

        Calories = ingredient.CaloriesPer100g * multiplier;
        Protein = ingredient.ProteinPer100g * multiplier;
        Fat = ingredient.FatPer100g * multiplier;
        Carbohydrates =
            ingredient.CarbohydratesPer100g * multiplier;
        Sugars = ingredient.SugarsPer100g * multiplier;
        Fiber = ingredient.FiberPer100g * multiplier;
        Salt = ingredient.SaltPer100g * multiplier;
    }

    public int Id { get; private set; }

    public int PreparedRecipeId { get; private set; }

    public string NameSnapshot { get; private set; } =
        string.Empty;

    public decimal Grams { get; private set; }

    public int Position { get; private set; }

    public decimal Calories { get; private set; }

    public decimal Protein { get; private set; }

    public decimal Fat { get; private set; }

    public decimal Carbohydrates { get; private set; }

    public decimal Sugars { get; private set; }

    public decimal Fiber { get; private set; }

    public decimal Salt { get; private set; }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Ingredient snapshot name cannot be empty.",
                nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Ingredient snapshot name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        return normalizedName;
    }
}