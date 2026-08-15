using System.ComponentModel.DataAnnotations;
using MealBuilder.Domain.Profiles;

namespace MealBuilder.Api.Contracts.Profiles;

public sealed record CalorieTargetCalculationRequest(
    [Required]
    DateOnly? BirthDate,

    [Required]
    [EnumDataType(typeof(CalculationSex))]
    CalculationSex? SexForCalculation,

    [Required]
    [Range(
        typeof(decimal),
        "100",
        "250")]
    decimal? HeightCm,

    [Required]
    [Range(
        typeof(decimal),
        "30",
        "400")]
    decimal? WeightKg,

    [Required]
    [EnumDataType(typeof(ActivityLevel))]
    ActivityLevel? ActivityLevel,

    [Required]
    [EnumDataType(typeof(WeightGoal))]
    WeightGoal? WeightGoal);