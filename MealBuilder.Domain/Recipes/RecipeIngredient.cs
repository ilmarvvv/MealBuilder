using MealBuilder.Domain.Ingredients;

namespace MealBuilder.Domain.Recipes;

public sealed class RecipeIngredient
{
    public const decimal MaxGrams = 100000m;

    private RecipeIngredient()
    {
    }

    internal RecipeIngredient(
        Ingredient ingredient,
        decimal grams,
        int position)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (ingredient.Id <= 0)
        {
            throw new ArgumentException(
                "Ingredient must already exist.",
                nameof(ingredient));
        }

        IngredientId = ingredient.Id;
        Ingredient = ingredient;
        Grams = ValidateGrams(grams);
        SetPosition(position);
    }

    public int Id { get; private set; }

    public int RecipeId { get; private set; }

    public int IngredientId { get; private set; }

    public Ingredient Ingredient { get; private set; } = null!;

    public decimal Grams { get; private set; }

    public int Position { get; private set; }

    internal void UpdateGrams(decimal grams)
    {
        Grams = ValidateGrams(grams);
    }

    internal void SetPosition(int position)
    {
        if (position <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "Position must be greater than zero.");
        }

        Position = position;
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
}