using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeIngredientGramsRequest(
    [Range(typeof(decimal), "0.01", "100000")]
    decimal Grams);