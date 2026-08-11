using MealBuilder.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class RecipeStepConfiguration
    : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(
        EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("RecipeSteps", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_RecipeSteps_Instruction",
                "length(trim(Instruction)) BETWEEN 1 AND 2000");

            tableBuilder.HasCheckConstraint(
                "CK_RecipeSteps_Position",
                "Position > 0");
        });

        builder.HasKey(recipeStep => recipeStep.Id);

        builder.Property(recipeStep => recipeStep.Instruction)
            .HasMaxLength(RecipeStep.MaxInstructionLength)
            .IsRequired();

        builder.Property(recipeStep => recipeStep.Position)
            .HasDefaultValue(1)
            .IsRequired();

        builder.HasIndex(recipeStep => new
        {
            recipeStep.RecipeId,
            recipeStep.Position
        });
    }
}