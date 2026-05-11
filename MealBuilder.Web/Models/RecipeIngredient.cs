using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class RecipeIngredient
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }

        public Recipe Recipe { get; set; } = null!;

        public int IngredientId { get; set; }

        public Ingredient Ingredient { get; set; } = null!;

        [Range(0.01, 100000)]
        public decimal Grams { get; set; }
    }
}
