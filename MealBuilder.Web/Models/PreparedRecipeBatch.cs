using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class PreparedRecipeBatch
    {
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }

        public Recipe? Recipe { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly CookedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Range(1, 1000)]
        public decimal TotalServings { get; set; }

        [Range(1, 365)]
        public int PlannedDays { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string RecipeNameSnapshot { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal TotalCaloriesSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal TotalProteinSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal TotalFiberSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal TotalSugarSnapshot { get; set; }

        [Range(0, 1000000)]
        public decimal TotalSaltSnapshot { get; set; }

        public List<PreparedRecipeBatchItem> Items { get; set; } = [];

        public List<MenuItem> MenuItems { get; set; } = [];

    }
}
