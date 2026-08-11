using MealBuilder.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class RecipeIngredientConfiguration
    : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(
        EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("RecipeIngredients", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_RecipeIngredients_Grams",
                "Grams > 0 AND Grams <= 100000");

            tableBuilder.HasCheckConstraint(
                "CK_RecipeIngredients_Position",
                "Position > 0");
        });

        builder.HasKey(recipeIngredient => recipeIngredient.Id);

        builder.Property(recipeIngredient => recipeIngredient.Grams)
            .HasConversion<double>()
            .IsRequired();

        builder.Property(recipeIngredient => recipeIngredient.Position)
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasIndex(recipeIngredient => new
        {
            recipeIngredient.RecipeId,
            recipeIngredient.IngredientId
        })
        .IsUnique();

        builder.HasIndex(recipeIngredient => new
        {
            recipeIngredient.RecipeId,
            recipeIngredient.Position
        });

        builder.HasOne(recipeIngredient => recipeIngredient.Ingredient)
            .WithMany()
            .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}