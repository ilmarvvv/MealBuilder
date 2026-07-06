namespace MealBuilder.Web.ViewModels
{
    public class RecipeContentSummary
    {
        public int Id { get; set; }

        public int Position { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Grams { get; set; }

        public decimal Calories { get; set; }

        public decimal Protein { get; set; }

        public decimal Fiber { get; set; }

        public decimal Sugar { get; set; }

        public decimal Salt { get; set; }
    }
}
