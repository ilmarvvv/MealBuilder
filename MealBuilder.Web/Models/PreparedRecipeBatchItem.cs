using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class PreparedRecipeBatchItem
    {
        public int Id { get; set; }

        [Required]
        public int PreparedRecipeBatchId { get; set; }

        public PreparedRecipeBatch? PreparedRecipeBatch { get; set; }

        [Required]
        public PreparedRecipeBatchItemType ItemType { get; set; }

        public int? SourceIngredientId { get; set; }

        public Ingredient? SourceIngredient { get; set; }

        public int? SourceRecipeId { get; set; }

        public Recipe? SourceRecipe { get; set; }

        [Required]
        [StringLength(100)]
        public string NameSnapshot { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Grams { get; set; }

        [Range(0, 1000000)]
        public decimal CaloriesSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal ProteinSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal FiberSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal SugarSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal SaltSnapshot { get; set; }

        [Range(1, 1000)]
        public int Position { get; set; } = 1;
    }
}
