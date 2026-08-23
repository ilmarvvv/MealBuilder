using MealBuilder.Domain.MealPlanning;

namespace MealBuilder.Api.Contracts.MealPlanning.DailyPlans;

public sealed record DailyPlanItemResponse(
    int Id,
    DailyPlanItemType ItemType,
    int? IngredientId,
    int? PreparedRecipeId,
    string Name,
    decimal? Grams,
    decimal? Portions,
    TimeOnly? PlannedTime,
    MealPlanningNutritionResponse Nutrition);