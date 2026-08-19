namespace MealBuilder.Api.Contracts.MealPlanning;

public sealed record MealPlanningNutritionResponse(
    decimal Calories,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal Sugars,
    decimal Fiber,
    decimal Salt);