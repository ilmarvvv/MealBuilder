using MealBuilder.Api.Contracts.MealPlanning;
using MealBuilder.Api.Contracts.MealPlanning.PreparedRecipes;
using MealBuilder.Domain.MealPlanning;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Mappings;

public static class PreparedRecipeResponseMapper
{
    public static PreparedRecipeSummaryResponse ToSummaryResponse(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions)
    {
        var availability = ToAvailabilityResponse(
            preparedRecipe,
            allocatedPortions);

        var nutritionPerPortion =
            PreparedRecipeNutritionCalculator
                .CalculatePerPortion(preparedRecipe);

        return new PreparedRecipeSummaryResponse(
            preparedRecipe.Id,
            preparedRecipe.SourceRecipeId,
            preparedRecipe.NameSnapshot,
            preparedRecipe.PreparedDate,
            availability.TotalPortions,
            availability.AllocatedPortions,
            availability.AvailablePortions,
            ToNutritionResponse(nutritionPerPortion));
    }

    public static PreparedRecipeResponse ToResponse(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions)
    {
        var availability = ToAvailabilityResponse(
            preparedRecipe,
            allocatedPortions);

        var totalNutrition =
            PreparedRecipeNutritionCalculator
                .CalculateTotal(preparedRecipe);

        var nutritionPerPortion =
            PreparedRecipeNutritionCalculator
                .CalculatePerPortion(preparedRecipe);

        var ingredients = preparedRecipe.Ingredients
            .OrderBy(ingredient => ingredient.Position)
            .Select(ingredient =>
                new PreparedRecipeIngredientResponse(
                    ingredient.Id,
                    ingredient.NameSnapshot,
                    ingredient.Grams,
                    ingredient.Position,
                    ToIngredientNutritionResponse(ingredient)))
            .ToArray();

        return new PreparedRecipeResponse(
            preparedRecipe.Id,
            preparedRecipe.SourceRecipeId,
            preparedRecipe.NameSnapshot,
            preparedRecipe.PreparedDate,
            availability.TotalPortions,
            availability.AllocatedPortions,
            availability.AvailablePortions,
            ToNutritionResponse(totalNutrition),
            ToNutritionResponse(nutritionPerPortion),
            ingredients);
    }

    public static PreparedRecipeAvailabilityResponse
        ToAvailabilityResponse(
            PreparedRecipe preparedRecipe,
            decimal allocatedPortions)
    {
        var availablePortions =
            PreparedRecipePortionCalculator
                .CalculateAvailablePortions(
                    preparedRecipe,
                    allocatedPortions);

        return new PreparedRecipeAvailabilityResponse(
            preparedRecipe.Id,
            preparedRecipe.TotalPortions,
            allocatedPortions,
            availablePortions);
    }

    public static PreparedRecipeDeletionImpactResponse
        ToDeletionImpactResponse(
            PreparedRecipe preparedRecipe,
            int affectedItemCount,
            int affectedDateCount)
    {
        return new PreparedRecipeDeletionImpactResponse(
            preparedRecipe.Id,
            preparedRecipe.NameSnapshot,
            affectedItemCount,
            affectedDateCount);
    }

    private static MealPlanningNutritionResponse
        ToIngredientNutritionResponse(
            PreparedRecipeIngredient ingredient)
    {
        return new MealPlanningNutritionResponse(
            Round(ingredient.Calories),
            Round(ingredient.Protein),
            Round(ingredient.Fat),
            Round(ingredient.Carbohydrates),
            Round(ingredient.Sugars),
            Round(ingredient.Fiber),
            Round(ingredient.Salt));
    }

    private static MealPlanningNutritionResponse
        ToNutritionResponse(
            RecipeNutrition nutrition)
    {
        return new MealPlanningNutritionResponse(
            Round(nutrition.Calories),
            Round(nutrition.Protein),
            Round(nutrition.Fat),
            Round(nutrition.Carbohydrates),
            Round(nutrition.Sugars),
            Round(nutrition.Fiber),
            Round(nutrition.Salt));
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}