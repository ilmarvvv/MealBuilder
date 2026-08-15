using MealBuilder.Domain.Profiles;
using MealBuilder.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class UserNutritionProfileConfiguration
    : IEntityTypeConfiguration<UserNutritionProfile>
{
    public void Configure(
        EntityTypeBuilder<UserNutritionProfile> builder)
    {
        builder.ToTable("UserNutritionProfiles", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_DailyCalorieTarget",
                $"DailyCalorieTarget BETWEEN " +
                $"{UserNutritionProfile.MinDailyCalorieTarget} AND " +
                $"{UserNutritionProfile.MaxDailyCalorieTarget}");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_HeightCm",
                $"HeightCm IS NULL OR HeightCm BETWEEN " +
                $"{UserNutritionProfile.MinHeightCm} AND " +
                $"{UserNutritionProfile.MaxHeightCm}");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_WeightKg",
                $"WeightKg IS NULL OR WeightKg BETWEEN " +
                $"{UserNutritionProfile.MinWeightKg} AND " +
                $"{UserNutritionProfile.MaxWeightKg}");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_SexForCalculation",
                "SexForCalculation IS NULL OR " +
                "SexForCalculation IN (1, 2)");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_ActivityLevel",
                "ActivityLevel IS NULL OR " +
                "ActivityLevel IN (1, 2, 3, 4)");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_WeightGoal",
                "WeightGoal IS NULL OR " +
                "WeightGoal IN (1, 2, 3)");

            tableBuilder.HasCheckConstraint(
                "CK_UserNutritionProfiles_CalculationInputs",
                "(BirthDate IS NULL AND " +
                "SexForCalculation IS NULL AND " +
                "HeightCm IS NULL AND " +
                "WeightKg IS NULL AND " +
                "ActivityLevel IS NULL AND " +
                "WeightGoal IS NULL) OR " +
                "(BirthDate IS NOT NULL AND " +
                "SexForCalculation IS NOT NULL AND " +
                "HeightCm IS NOT NULL AND " +
                "WeightKg IS NOT NULL AND " +
                "ActivityLevel IS NOT NULL AND " +
                "WeightGoal IS NOT NULL)");
        });

        builder.HasKey(profile => profile.UserId);

        builder.Property(profile => profile.UserId)
            .HasMaxLength(450)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(profile => profile.DailyCalorieTarget)
            .IsRequired();

        builder.Property(profile => profile.HeightCm)
            .HasConversion<double?>();

        builder.Property(profile => profile.WeightKg)
            .HasConversion<double?>();

        builder.Ignore(profile => profile.HasCalculationInputs);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserNutritionProfile>(
                profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}