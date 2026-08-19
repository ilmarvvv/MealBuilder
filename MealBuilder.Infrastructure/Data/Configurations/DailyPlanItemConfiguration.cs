using MealBuilder.Domain.MealPlanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class DailyPlanItemConfiguration
    : IEntityTypeConfiguration<DailyPlanItem>
{
    public void Configure(
        EntityTypeBuilder<DailyPlanItem> builder)
    {
        builder.ToTable("DailyPlanItems", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_DailyPlanItems_ItemType",
                "ItemType IN (1, 2)");

            tableBuilder.HasCheckConstraint(
                "CK_DailyPlanItems_Grams",
                "Grams IS NULL OR " +
                "(Grams > 0 AND Grams <= 100000)");

            tableBuilder.HasCheckConstraint(
                "CK_DailyPlanItems_Portions",
                "Portions IS NULL OR " +
                "(Portions > 0 AND " +
                "Portions = ROUND(Portions, 2))");

            tableBuilder.HasCheckConstraint(
                "CK_DailyPlanItems_SourceAndQuantity",
                "(ItemType = 1 AND " +
                "IngredientId IS NOT NULL AND " +
                "PreparedRecipeId IS NULL AND " +
                "Grams IS NOT NULL AND " +
                "Portions IS NULL) OR " +
                "(ItemType = 2 AND " +
                "IngredientId IS NULL AND " +
                "PreparedRecipeId IS NOT NULL AND " +
                "Grams IS NULL AND " +
                "Portions IS NOT NULL)");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.DailyPlanId)
            .IsRequired();

        builder.Property(item => item.ItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.Grams)
            .HasConversion<double?>();

        builder.Property(item => item.Portions)
            .HasConversion<double?>();

        builder.Property(item => item.PlannedTime);

        builder.HasIndex(item => new
        {
            item.DailyPlanId,
            item.PlannedTime,
            item.Id
        });

        builder.HasIndex(item =>
            item.IngredientId);

        builder.HasIndex(item =>
            item.PreparedRecipeId);

        builder.HasOne(item => item.Ingredient)
            .WithMany()
            .HasForeignKey(item =>
                item.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.PreparedRecipe)
            .WithMany()
            .HasForeignKey(item =>
                item.PreparedRecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}