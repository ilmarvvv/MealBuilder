namespace MealBuilder.Domain.MealPlanning;

public static class PreparedRecipePortionDistributor
{
    public const int MaxPlannedDays = 365;

    public static IReadOnlyList<
        PreparedRecipeAllocationProposal> CreateDistribution(
        PreparedRecipe preparedRecipe,
        DateOnly startDate,
        int plannedDays)
    {
        ArgumentNullException.ThrowIfNull(preparedRecipe);

        ValidateDates(
            preparedRecipe,
            startDate,
            plannedDays);

        var totalHundredths =
            preparedRecipe.TotalPortions * 100m;

        if (totalHundredths < plannedDays)
        {
            throw new ArgumentException(
                "Every planned day must receive at least 0.01 portion.",
                nameof(plannedDays));
        }

        var baseHundredths = decimal.Floor(
            totalHundredths / plannedDays);

        var remainderHundredths = decimal.ToInt32(
            totalHundredths -
            baseHundredths * plannedDays);

        var distribution =
            new List<PreparedRecipeAllocationProposal>(
                plannedDays);

        for (var index = 0; index < plannedDays; index++)
        {
            var portionHundredths =
                baseHundredths +
                (index < remainderHundredths ? 1m : 0m);

            distribution.Add(
                new PreparedRecipeAllocationProposal(
                    startDate.AddDays(index),
                    portionHundredths / 100m));
        }

        return distribution.AsReadOnly();
    }

    private static void ValidateDates(
        PreparedRecipe preparedRecipe,
        DateOnly startDate,
        int plannedDays)
    {
        if (plannedDays <= 0 ||
            plannedDays > MaxPlannedDays)
        {
            throw new ArgumentOutOfRangeException(
                nameof(plannedDays),
                plannedDays,
                $"Planned days must be between 1 and {MaxPlannedDays}.");
        }

        if (startDate < preparedRecipe.PreparedDate)
        {
            throw new ArgumentException(
                "Planning cannot start before the prepared date.",
                nameof(startDate));
        }

        var lastDayOffset = plannedDays - 1;

        if (startDate.DayNumber >
            DateOnly.MaxValue.DayNumber - lastDayOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startDate),
                startDate,
                "The planned date range exceeds the supported calendar.");
        }
    }
}