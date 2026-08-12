using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeRequest(
    [Required]
    [StringLength(Recipe.MaxNameLength)]
    string Name,

    [StringLength(Recipe.MaxDescriptionLength)]
    string? Description,

    [Range(1, Recipe.MaxServings)]
    int Servings,

    [Required]
    [MinLength(1)]
    RecipeIngredientRequest[] Ingredients,

    [Required]
    [MinLength(1)]
    RecipeStepRequest[] Steps);