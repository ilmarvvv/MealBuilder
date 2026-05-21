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

        public List<MenuItem> MenuItems { get; set; } = [];

    }
}
