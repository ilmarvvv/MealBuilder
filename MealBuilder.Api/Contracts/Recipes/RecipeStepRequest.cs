using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.Recipes;

namespace MealBuilder.Api.Contracts.Recipes;

public sealed record RecipeStepRequest(
    [Required]
    [StringLength(RecipeStep.MaxInstructionLength)]
    string Instruction);