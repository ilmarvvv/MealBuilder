using MealBuilder.Domain.Ingredients;
using MealBuilder.Domain.MealPlanning;
using MealBuilder.Domain.Recipes;
using MealBuilder.Domain.Profiles;
using MealBuilder.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients => Set<Ingredient>();

        public DbSet<Recipe> Recipes => Set<Recipe>();

        public DbSet<RecipeIngredient> RecipeIngredients =>
            Set<RecipeIngredient>();

        public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

        public DbSet<PreparedRecipe> PreparedRecipes =>
            Set<PreparedRecipe>();

        public DbSet<PreparedRecipeIngredient>
            PreparedRecipeIngredients =>
                Set<PreparedRecipeIngredient>();

        public DbSet<DailyPlan> DailyPlans =>
            Set<DailyPlan>();

        public DbSet<DailyPlanItem> DailyPlanItems =>
            Set<DailyPlanItem>();

        public DbSet<UserNutritionProfile> UserNutritionProfiles =>
            Set<UserNutritionProfile>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
    }
}