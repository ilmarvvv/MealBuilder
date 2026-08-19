namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record PreparedRecipeIngredientResponse(
    int Id,
    string Name,
    decimal Grams,
    int Position,
    MealPlanningNutritionResponse Nutrition);