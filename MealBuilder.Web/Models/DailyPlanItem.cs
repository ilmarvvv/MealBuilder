using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class DailyPlanItem
    {
        public int Id { get; set; }

        public int DailyPlanId { get; set; }

        public DailyPlan DailyPlan { get; set; } = null!;

        public DailyPlanItemType ItemType { get; set; }

        public int? RecipeId { get; set; }

        public Recipe? Recipe { get; set; }

        public int? PreparedRecipeBatchId { get; set; }

        public PreparedRecipeBatch? PreparedRecipeBatch { get; set; }

        public int? IngredientId { get; set; }

        public Ingredient? Ingredient { get; set; }

        [Range(0.01, 100000)]
        public decimal? ServingsCount { get; set; }

        [Range(0.01, 100000)]
        public decimal? Grams { get; set; }
    }
}
