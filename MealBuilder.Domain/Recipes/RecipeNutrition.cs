namespace MealBuilder.Domain.Recipes;

public readonly record struct RecipeNutrition(
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal Sugars,
    decimal Fiber,
    decimal Salt)
{
    public static RecipeNutrition Zero { get; } = new(
        Calories: 0,
        Protein: 0,
        Fat: 0,
        Carbohydrates: 0,
        Sugars: 0,
        Fiber: 0,
        Salt: 0);

    public RecipeNutrition Add(RecipeNutrition nutrition)
    {
        return new RecipeNutrition(
            Calories + nutrition.Calories,
            Protein + nutrition.Protein,
            Fat + nutrition.Fat,
            Carbohydrates + nutrition.Carbohydrates,
            Sugars + nutrition.Sugars,
            Fiber + nutrition.Fiber,
            Salt + nutrition.Salt);
    }

    public RecipeNutrition MultiplyBy(decimal multiplier)
    {
        if (multiplier < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(multiplier),
                multiplier,
                "Multiplier cannot be negative.");
        }

        return new RecipeNutrition(
            Calories * multiplier,
            Protein * multiplier,
            Fat * multiplier,
            Carbohydrates * multiplier,
            Sugars * multiplier,
            Fiber * multiplier,
            Salt * multiplier);
    }

    public RecipeNutrition DivideBy(int divisor)
    {
        return DivideBy((decimal)divisor);
    }

    public RecipeNutrition DivideBy(decimal divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(divisor),
                divisor,
                "Divisor must be greater than zero.");
        }

        return new RecipeNutrition(
            Calories / divisor,
            Protein / divisor,
            Fat / divisor,
            Carbohydrates / divisor,
            Sugars / divisor,
            Fiber / divisor,
            Salt / divisor);
    }
}