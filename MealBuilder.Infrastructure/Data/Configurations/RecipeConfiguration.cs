using MealBuilder.Domain.Recipes;
using MealBuilder.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class RecipeConfiguration
    : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Recipes_Name",
                "length(trim(Name)) BETWEEN 1 AND 100");

            tableBuilder.HasCheckConstraint(
                "CK_Recipes_Servings",
                "Servings BETWEEN 1 AND 100");
        });

        builder.HasKey(recipe => recipe.Id);

        builder.Property(recipe => recipe.OwnerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(recipe => recipe.Name)
            .HasMaxLength(Recipe.MaxNameLength)
            .IsRequired();

        builder.Property(recipe => recipe.Description)
            .HasMaxLength(Recipe.MaxDescriptionLength);

        builder.Property(recipe => recipe.Servings)
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasIndex(recipe => recipe.OwnerId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(recipe => recipe.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(recipe => recipe.Ingredients)
            .WithOne()
            .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(recipe => recipe.Steps)
            .WithOne()
            .HasForeignKey(recipeStep => recipeStep.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(recipe => recipe.Ingredients)
            .HasField("_ingredients")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(recipe => recipe.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}