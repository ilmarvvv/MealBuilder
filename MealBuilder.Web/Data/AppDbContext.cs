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

        public DbSet<PreparedRecipeBatch> PreparedRecipeBatches { get; set; }

        public DbSet<PreparedRecipeBatchItem> PreparedRecipeBatchItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(recipeIngredient => recipeIngredient.Ingredient)
                .WithMany(ingredient => ingredient.RecipeIngredients)
                .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<MenuItem>()
                .HasOne(menuItem => menuItem.PreparedRecipeBatch)
                .WithMany(preparedRecipeBatch => preparedRecipeBatch.MenuItems)
                .HasForeignKey(menuItem => menuItem.PreparedRecipeBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PreparedRecipeBatch>()
                .HasOne(preparedRecipeBatch => preparedRecipeBatch.Recipe)
                .WithMany(recipe => recipe.PreparedRecipeBatches)
                .HasForeignKey(preparedRecipeBatch => preparedRecipeBatch.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PreparedRecipeBatchItem>()
                .HasOne(preparedRecipeBatchItem => preparedRecipeBatchItem.PreparedRecipeBatch)
                .WithMany(preparedRecipeBatch => preparedRecipeBatch.Items)
                .HasForeignKey(preparedRecipeBatchItem => preparedRecipeBatchItem.PreparedRecipeBatchId);

            modelBuilder.Entity<PreparedRecipeBatchItem>()
                .HasOne(preparedRecipeBatchItem => preparedRecipeBatchItem.SourceIngredient)
                .WithMany()
                .HasForeignKey(preparedRecipeBatchItem => preparedRecipeBatchItem.SourceIngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PreparedRecipeBatchItem>()
                .HasOne(preparedRecipeBatchItem => preparedRecipeBatchItem.SourceRecipe)
                .WithMany()
                .HasForeignKey(preparedRecipeBatchItem => preparedRecipeBatchItem.SourceRecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
