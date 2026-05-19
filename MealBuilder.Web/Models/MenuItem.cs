using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        public int MenuId { get; set; }

        public Menu Menu { get; set; } = null!;

        public MenuItemType ItemType { get; set; }

        public int? RecipeId { get; set; }

        public Recipe? Recipe { get; set; }

        public int? IngredientId { get; set; }

        public Ingredient? Ingredient { get; set; }

        [Range(0.01, 100000)]
        public decimal? ServingsCount { get; set; }

        [Range(0.01, 100000)]
        public decimal? Grams { get; set; }
    }
}
