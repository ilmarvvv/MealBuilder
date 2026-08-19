using MealBuilder.Domain.MealPlanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class PreparedRecipeIngredientConfiguration
    : IEntityTypeConfiguration<PreparedRecipeIngredient>
{
    public void Configure(
        EntityTypeBuilder<PreparedRecipeIngredient> builder)
    {
        builder.ToTable(
            "PreparedRecipeIngredients",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PreparedRecipeIngredients_NameSnapshot",
                    "length(trim(NameSnapshot)) BETWEEN 1 AND 100");

                tableBuilder.HasCheckConstraint(
                    "CK_PreparedRecipeIngredients_Grams",
                    "Grams > 0 AND Grams <= 100000");

                tableBuilder.HasCheckConstraint(
                    "CK_PreparedRecipeIngredients_Position",
                    "Position > 0");

                tableBuilder.HasCheckConstraint(
                    "CK_PreparedRecipeIngredients_Nutrition",
                    "Calories >= 0 AND " +
                    "Protein >= 0 AND " +
                    "Fat >= 0 AND " +
                    "Carbohydrates >= 0 AND " +
                    "Sugars >= 0 AND " +
                    "Fiber >= 0 AND " +
                    "Salt >= 0");

                tableBuilder.HasCheckConstraint(
                    "CK_PreparedRecipeIngredients_Sugars",
                    "Sugars <= Carbohydrates");
            });

        builder.HasKey(ingredient => ingredient.Id);

        builder.Property(ingredient =>
                ingredient.PreparedRecipeId)
            .IsRequired();

        builder.Property(ingredient =>
                ingredient.NameSnapshot)
            .HasMaxLength(
                PreparedRecipeIngredient.MaxNameLength)
            .IsRequired();

        builder.Property(ingredient => ingredient.Grams)
            .HasConversion<double>()
            .IsRequired();

        builder.Property(ingredient => ingredient.Position)
            .IsRequired();

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Calories));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Protein));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Fat));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Carbohydrates));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Sugars));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Fiber));

        ConfigureNutrition(
            builder.Property(ingredient =>
                ingredient.Salt));

        builder.HasIndex(ingredient => new
        {
            ingredient.PreparedRecipeId,
            ingredient.Position
        })
        .IsUnique();
    }

    private static void ConfigureNutrition(
        PropertyBuilder<decimal> property)
    {
        property
            .HasConversion<double>()
            .HasDefaultValue(0m)
            .IsRequired();
    }
}