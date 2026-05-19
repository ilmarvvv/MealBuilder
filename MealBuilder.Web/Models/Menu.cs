using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Web.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(1000)]
        public string? Description { get; set; }

        public List<MenuItem> MenuItems { get; set; } = [];
    }
}
