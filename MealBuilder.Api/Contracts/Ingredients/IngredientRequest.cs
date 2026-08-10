using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.Ingredients;

namespace MealBuilder.Api.Contracts.Ingredients;

public sealed record IngredientRequest(
    [Required]
    [StringLength(Ingredient.MaxNameLength)]
    string Name,

    [Range(typeof(decimal), "0", "900")]
    decimal CaloriesPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal ProteinPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal FatPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal CarbohydratesPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal SugarsPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal FiberPer100g,

    [Range(typeof(decimal), "0", "100")]
    decimal SaltPer100g)
    : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SugarsPer100g > CarbohydratesPer100g)
        {
            yield return new ValidationResult(
                "Sugars cannot exceed carbohydrates.",
                [nameof(SugarsPer100g)]);
        }
    }
}