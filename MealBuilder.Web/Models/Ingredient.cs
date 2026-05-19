using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models;

public class Ingredient
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 900)]
    public decimal CaloriesPer100g { get; set; }

    [Range(0, 100)]
    public decimal ProteinPer100g { get; set; }

    [Range(0, 100)]
    public decimal FiberPer100g { get; set; }

    [Range(0, 100)]
    public decimal SugarPer100g { get; set; }

    [Range(0, 100)]
    public decimal SaltPer100g { get; set; }

    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];

    public List<MenuItem> MenuItems { get; set; } = [];
}
