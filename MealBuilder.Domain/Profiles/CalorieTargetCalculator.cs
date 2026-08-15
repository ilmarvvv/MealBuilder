namespace MealBuilder.Domain.Profiles;

public static class CalorieTargetCalculator
{
    public static CalorieTargetEstimate Calculate(
        DateOnly birthDate,
        CalculationSex sexForCalculation,
        decimal heightCm,
        decimal weightKg,
        ActivityLevel activityLevel,
        WeightGoal weightGoal,
        DateOnly currentDate)
    {
        var age = CalculateAge(birthDate, currentDate);

        ValidateRange(
            heightCm,
            UserNutritionProfile.MinHeightCm,
            UserNutritionProfile.MaxHeightCm,
            nameof(heightCm));

        ValidateRange(
            weightKg,
            UserNutritionProfile.MinWeightKg,
            UserNutritionProfile.MaxWeightKg,
            nameof(weightKg));

        var sexAdjustment = sexForCalculation switch
        {
            CalculationSex.Female => -161m,
            CalculationSex.Male => 5m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(sexForCalculation),
                sexForCalculation,
                "Calculation sex is invalid.")
        };

        var activityFactor = activityLevel switch
        {
            ActivityLevel.LowActive => 1.4m,
            ActivityLevel.ModeratelyActive => 1.6m,
            ActivityLevel.Active => 1.8m,
            ActivityLevel.VeryActive => 2.0m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(activityLevel),
                activityLevel,
                "Activity level is invalid.")
        };

        var goalFactor = weightGoal switch
        {
            WeightGoal.LoseWeight => 0.9m,
            WeightGoal.MaintainWeight => 1m,
            WeightGoal.GainWeight => 1.1m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(weightGoal),
                weightGoal,
                "Weight goal is invalid.")
        };

        var restingEnergyExpenditure =
            10m * weightKg +
            6.25m * heightCm -
            5m * age +
            sexAdjustment;

        var maintenanceCalories =
            restingEnergyExpenditure * activityFactor;

        var recommendedDailyCalorieTarget = Round(
            maintenanceCalories * goalFactor);

        if (
            recommendedDailyCalorieTarget <
                UserNutritionProfile.MinDailyCalorieTarget ||
            recommendedDailyCalorieTarget >
                UserNutritionProfile.MaxDailyCalorieTarget)
        {
            throw new InvalidOperationException(
                "The calculated calorie target is outside " +
                "the supported range.");
        }

        return new CalorieTargetEstimate(
            age,
            Round(restingEnergyExpenditure),
            Round(maintenanceCalories),
            recommendedDailyCalorieTarget);
    }

    private static int CalculateAge(
        DateOnly birthDate,
        DateOnly currentDate)
    {
        if (birthDate > currentDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                birthDate,
                "Birth date cannot be in the future.");
        }

        var age = currentDate.Year - birthDate.Year;

        if (birthDate > currentDate.AddYears(-age))
        {
            age--;
        }

        if (
            age < UserNutritionProfile.MinAge ||
            age > UserNutritionProfile.MaxAge)
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                birthDate,
                $"Age must be between " +
                $"{UserNutritionProfile.MinAge} and " +
                $"{UserNutritionProfile.MaxAge}.");
        }

        return age;
    }

    private static void ValidateRange(
        decimal value,
        decimal minimum,
        decimal maximum,
        string parameterName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Value must be between {minimum} and {maximum}.");
        }
    }

    private static int Round(decimal value)
    {
        return (int)decimal.Round(
            value,
            decimals: 0,
            MidpointRounding.AwayFromZero);
    }
}