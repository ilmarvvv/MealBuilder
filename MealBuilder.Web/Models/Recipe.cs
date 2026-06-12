using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, 365)]
        public int DefaultPlannedDays { get; set; } = 1;

        [Range(1, 100)]
        public int DefaultServingsPerDay { get; set; } = 1;

        [Range(0, 10000)]
        public int PrepTimeMinutes { get; set; }

        [Range(0, 10000)]
        public int CookTimeMinutes { get; set; }

        [Range(1, 100)]
        public int Servings { get; set; } = 1;

        public List<RecipeIngredient> RecipeIngredients { get; set; } = [];

        public List<RecipeComponent> Components { get; set; } = [];

        public List<RecipeComponent> UsedAsComponentInRecipes { get; set; } = [];

        public List<MenuItem> MenuItems { get; set; } = [];

        public List<PreparedRecipeBatch> PreparedRecipeBatches { get; set; } = [];

    }
}
