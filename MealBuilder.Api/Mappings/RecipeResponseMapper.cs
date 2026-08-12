using MealBuilder.Api.Contracts.Recipes;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Mappings;

public static class RecipeResponseMapper
{
    public static RecipeSummaryResponse ToSummaryResponse(
        Recipe recipe)
    {
        var nutritionPerServing =
            RecipeNutritionCalculator.CalculatePerServing(recipe);

        return new RecipeSummaryResponse(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Servings,
            recipe.Ingredients.Count,
            ToNutritionResponse(nutritionPerServing));
    }

    public static RecipeResponse ToResponse(Recipe recipe)
    {
        var totalNutrition =
            RecipeNutritionCalculator.CalculateTotal(recipe);

        var nutritionPerServing =
            RecipeNutritionCalculator.CalculatePerServing(recipe);

        var ingredients = recipe.Ingredients
            .OrderBy(recipeIngredient => recipeIngredient.Position)
            .Select(recipeIngredient =>
                new RecipeIngredientResponse(
                    recipeIngredient.IngredientId,
                    recipeIngredient.Ingredient.Name,
                    recipeIngredient.Grams,
                    recipeIngredient.Position))
            .ToArray();

        var steps = recipe.Steps
            .OrderBy(recipeStep => recipeStep.Position)
            .Select(recipeStep =>
                new RecipeStepResponse(
                    recipeStep.Id,
                    recipeStep.Instruction,
                    recipeStep.Position))
            .ToArray();

        return new RecipeResponse(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Servings,
            ToNutritionResponse(totalNutrition),
            ToNutritionResponse(nutritionPerServing),
            ingredients,
            steps);
    }

    private static RecipeNutritionResponse ToNutritionResponse(
        RecipeNutrition nutrition)
    {
        return new RecipeNutritionResponse(
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