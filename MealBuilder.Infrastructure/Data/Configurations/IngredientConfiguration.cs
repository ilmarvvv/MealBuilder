using MealBuilder.Domain.Ingredients;
using MealBuilder.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class IngredientConfiguration
    : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("Ingredients", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Ingredients_Name",
                "length(trim(Name)) BETWEEN 1 AND 100");

            tableBuilder.HasCheckConstraint(
                "CK_Ingredients_CaloriesPer100g",
                "CaloriesPer100g BETWEEN 0 AND 900");

            tableBuilder.HasCheckConstraint(
                "CK_Ingredients_NutrientsPer100g",
                "ProteinPer100g BETWEEN 0 AND 100 AND " +
                "FatPer100g BETWEEN 0 AND 100 AND " +
                "CarbohydratesPer100g BETWEEN 0 AND 100 AND " +
                "SugarsPer100g BETWEEN 0 AND 100 AND " +
                "FiberPer100g BETWEEN 0 AND 100 AND " +
                "SaltPer100g BETWEEN 0 AND 100");

            tableBuilder.HasCheckConstraint(
                "CK_Ingredients_Sugars",
                "SugarsPer100g <= CarbohydratesPer100g");

            tableBuilder.HasCheckConstraint(
                "CK_Ingredients_Ownership",
                "(OwnerId IS NULL AND " +
                "SourceName IS NOT NULL AND " +
                "SourceCode IS NOT NULL AND " +
                "SourceVersion IS NOT NULL) OR " +
                "(OwnerId IS NOT NULL AND " +
                "SourceName IS NULL AND " +
                "SourceCode IS NULL AND " +
                "SourceVersion IS NULL)");
        });

        builder.HasKey(ingredient => ingredient.Id);

        builder.Property(ingredient => ingredient.Name)
            .HasMaxLength(Ingredient.MaxNameLength)
            .IsRequired();

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.CaloriesPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.ProteinPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.FatPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.CarbohydratesPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.SugarsPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.FiberPer100g));

        ConfigureNutrition(
            builder.Property(ingredient => ingredient.SaltPer100g));

        builder.Property(ingredient => ingredient.OwnerId)
            .HasMaxLength(450);

        builder.Property(ingredient => ingredient.SourceName)
            .HasMaxLength(100);

        builder.Property(ingredient => ingredient.SourceCode)
            .HasMaxLength(100);

        builder.Property(ingredient => ingredient.SourceVersion)
            .HasMaxLength(50);

        builder.Ignore(ingredient => ingredient.IsBuiltIn);

        builder.HasIndex(ingredient => ingredient.Name);

        builder.HasIndex(ingredient => ingredient.OwnerId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(ingredient => ingredient.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
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