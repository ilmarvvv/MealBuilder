namespace MealBuilder.Domain.MealPlanning;

public static class PreparedRecipePortionCalculator
{
    public static decimal CalculateAvailablePortions(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions)
    {
        ArgumentNullException.ThrowIfNull(preparedRecipe);

        ValidateAllocatedPortions(
            preparedRecipe,
            allocatedPortions);

        return preparedRecipe.TotalPortions -
            allocatedPortions;
    }

    public static void EnsureCanAllocate(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions,
        decimal requestedPortions)
    {
        ValidatePortions(
            requestedPortions,
            nameof(requestedPortions));

        var availablePortions = CalculateAvailablePortions(
            preparedRecipe,
            allocatedPortions);

        if (requestedPortions > availablePortions)
        {
            throw new InvalidOperationException(
                "The requested portions exceed the available portions.");
        }
    }

    public static void EnsureCanChangeAllocation(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions,
        decimal currentItemPortions,
        decimal requestedItemPortions)
    {
        ValidatePortions(
            currentItemPortions,
            nameof(currentItemPortions));

        ValidatePortions(
            requestedItemPortions,
            nameof(requestedItemPortions));

        ValidateAllocatedPortions(
            preparedRecipe,
            allocatedPortions);

        if (currentItemPortions > allocatedPortions)
        {
            throw new InvalidOperationException(
                "The current item portions exceed the allocated portions.");
        }

        var proposedAllocatedPortions =
            allocatedPortions -
            currentItemPortions +
            requestedItemPortions;

        if (proposedAllocatedPortions >
            preparedRecipe.TotalPortions)
        {
            throw new InvalidOperationException(
                "The requested change exceeds the available portions.");
        }
    }

    private static void ValidateAllocatedPortions(
        PreparedRecipe preparedRecipe,
        decimal allocatedPortions)
    {
        if (allocatedPortions < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(allocatedPortions),
                allocatedPortions,
                "Allocated portions cannot be negative.");
        }

        if (decimal.Round(allocatedPortions, 2) !=
            allocatedPortions)
        {
            throw new ArgumentException(
                "Allocated portions cannot have more than two decimal places.",
                nameof(allocatedPortions));
        }

        if (allocatedPortions >
            preparedRecipe.TotalPortions)
        {
            throw new InvalidOperationException(
                "Allocated portions cannot exceed total portions.");
        }
    }

    private static void ValidatePortions(
        decimal portions,
        string parameterName)
    {
        if (portions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                portions,
                "Portions must be greater than zero.");
        }

        if (decimal.Round(portions, 2) != portions)
        {
            throw new ArgumentException(
                "Portions cannot have more than two decimal places.",
                parameterName);
        }
    }
}