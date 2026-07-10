using Microsoft.AspNetCore.Identity;
using MealBuilder.Web.Models;

namespace MealBuilder.Web.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public List<Ingredient> Ingredients { get; set; } = [];
    }
}
