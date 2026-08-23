namespace MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;

public sealed record PreparedRecipeIngredientResponse(
    int Id,
    string Name,
    decimal Grams,
    int Position,
    MealPlanningNutritionResponse Nutrition);