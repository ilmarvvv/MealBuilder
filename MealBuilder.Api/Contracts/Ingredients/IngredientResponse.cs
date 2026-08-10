namespace MealBuilder.Api.Contracts.Ingredients;

public sealed record IngredientResponse(
    int Id,
    string Name,
    decimal CaloriesPer100g,
    decimal ProteinPer100g,
    decimal FatPer100g,
    decimal CarbohydratesPer100g,
    decimal SugarsPer100g,
    decimal FiberPer100g,
    decimal SaltPer100g,
    bool IsBuiltIn,
    string? SourceName,
    string? SourceCode,
    string? SourceVersion);