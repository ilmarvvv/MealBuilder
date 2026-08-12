using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeIngredientRequest(
    [Range(1, int.MaxValue)]
    int IngredientId,

    [Range(typeof(decimal), "0.01", "100000")]
    decimal Grams);