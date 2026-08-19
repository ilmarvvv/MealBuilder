using MealBuilder.Domain.MealPlanning;
using MealBuilder.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class PreparedRecipeConfiguration
    : IEntityTypeConfiguration<PreparedRecipe>
{
    public void Configure(
        EntityTypeBuilder<PreparedRecipe> builder)
    {
        builder.ToTable("PreparedRecipes", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_PreparedRecipes_NameSnapshot",
                "length(trim(NameSnapshot)) BETWEEN 1 AND 100");

            tableBuilder.HasCheckConstraint(
                "CK_PreparedRecipes_TotalPortions",
                "TotalPortions > 0");
        });

        builder.HasKey(preparedRecipe =>
            preparedRecipe.Id);

        builder.Property(preparedRecipe =>
                preparedRecipe.OwnerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(preparedRecipe =>
                preparedRecipe.NameSnapshot)
            .HasMaxLength(PreparedRecipe.MaxNameLength)
            .IsRequired();

        builder.Property(preparedRecipe =>
                preparedRecipe.PreparedDate)
            .IsRequired();

        builder.Property(preparedRecipe =>
                preparedRecipe.TotalPortions)
            .HasConversion<double>()
            .IsRequired();

        builder.HasIndex(preparedRecipe => new
        {
            preparedRecipe.OwnerId,
            preparedRecipe.PreparedDate
        });

        builder.HasIndex(preparedRecipe => new
        {
            preparedRecipe.OwnerId,
            preparedRecipe.NameSnapshot
        });

        builder.HasIndex(preparedRecipe =>
            preparedRecipe.SourceRecipeId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(preparedRecipe =>
                preparedRecipe.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(preparedRecipe =>
                preparedRecipe.SourceRecipe)
            .WithMany()
            .HasForeignKey(preparedRecipe =>
                preparedRecipe.SourceRecipeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(preparedRecipe =>
                preparedRecipe.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient =>
                ingredient.PreparedRecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(preparedRecipe =>
                preparedRecipe.Ingredients)
            .HasField("_ingredients")
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}