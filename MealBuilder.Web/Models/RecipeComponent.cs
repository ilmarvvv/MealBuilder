using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class RecipeComponent
    {
        public int Id { get; set; }

        public int ParentRecipeId { get; set; }

        public Recipe ParentRecipe { get; set; } = null!;

        public int ComponentRecipeId { get; set; }

        public Recipe ComponentRecipe { get; set; } = null!;

        [Range(0.01, 100000)]
        public decimal Grams { get; set; }

        [Range(1, 1000)]
        public int Position { get; set; } = 1;
    }
}
