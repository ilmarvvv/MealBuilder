using MealBuilder.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MealBuilder.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Ingredient> Ingredients { get; set; }

        public DbSet<Recipe> Recipes { get; set; }

        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        public DbSet<RecipeComponent> RecipeComponents { get; set; }

        public DbSet<Menu> Menus { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecipeComponent>()
                .HasOne(recipeComponent => recipeComponent.ParentRecipe)
                .WithMany(recipe => recipe.Components)
                .HasForeignKey(recipeComponent => recipeComponent.ParentRecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecipeComponent>()
                .HasOne(recipeComponent => recipeComponent.ComponentRecipe)
                .WithMany(recipe => recipe.UsedAsComponentInRecipes)
                .HasForeignKey(recipeComponent => recipeComponent.ComponentRecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecipeComponent>()
                .HasIndex(recipeComponent => new
                {
                    recipeComponent.ParentRecipeId,
                    recipeComponent.ComponentRecipeId
                })
                .IsUnique();

            modelBuilder.Entity<Menu>()
                .HasIndex(menu => menu.Date)
                .IsUnique();

            modelBuilder.Entity<MenuItem>()
                .HasOne(menuItem => menuItem.Menu)
                .WithMany(menu => menu.MenuItems)
                .HasForeignKey(menuItem => menuItem.MenuId);

            modelBuilder.Entity<MenuItem>()
                .HasOne(menuItem => menuItem.Recipe)
                .WithMany(recipe => recipe.MenuItems)
                .HasForeignKey(menuItem => menuItem.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuItem>()
                .HasOne(menuItem => menuItem.Ingredient)
                .WithMany(ingredient => ingredient.MenuItems)
                .HasForeignKey(menuItem => menuItem.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
