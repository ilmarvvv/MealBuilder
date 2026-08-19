using MealBuilder.Domain.MealPlanning;
using MealBuilder.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealBuilder.Infrastructure.Data.Configurations;

public sealed class DailyPlanConfiguration
    : IEntityTypeConfiguration<DailyPlan>
{
    public void Configure(
        EntityTypeBuilder<DailyPlan> builder)
    {
        builder.ToTable("DailyPlans");

        builder.HasKey(dailyPlan => dailyPlan.Id);

        builder.Property(dailyPlan => dailyPlan.OwnerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(dailyPlan => dailyPlan.Date)
            .IsRequired();

        builder.Property(dailyPlan =>
                dailyPlan.IncludeInWeeklySummary)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Ignore(dailyPlan => dailyPlan.IsEmpty);

        builder.HasIndex(dailyPlan => new
        {
            dailyPlan.OwnerId,
            dailyPlan.Date
        })
        .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(dailyPlan =>
                dailyPlan.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(dailyPlan => dailyPlan.Items)
            .WithOne()
            .HasForeignKey(item => item.DailyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(dailyPlan => dailyPlan.Items)
            .HasField("_items")
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}