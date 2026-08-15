namespace MealBuilder.Domain.Profiles;

public sealed class UserNutritionProfile
{
    public const int MinAge = 18;
    public const int MaxAge = 100;
    public const decimal MinHeightCm = 100m;
    public const decimal MaxHeightCm = 250m;
    public const decimal MinWeightKg = 30m;
    public const decimal MaxWeightKg = 400m;
    public const int MinDailyCalorieTarget = 1000;
    public const int MaxDailyCalorieTarget = 10000;

    private UserNutritionProfile()
    {
    }

    private UserNutritionProfile(
        string userId,
        int dailyCalorieTarget)
    {
        UserId = NormalizeUserId(userId);
        DailyCalorieTarget =
            ValidateDailyCalorieTarget(dailyCalorieTarget);
    }

    public string UserId { get; private set; } = string.Empty;

    public int DailyCalorieTarget { get; private set; }

    public DateOnly? BirthDate { get; private set; }

    public CalculationSex? SexForCalculation { get; private set; }

    public decimal? HeightCm { get; private set; }

    public decimal? WeightKg { get; private set; }

    public ActivityLevel? ActivityLevel { get; private set; }

    public WeightGoal? WeightGoal { get; private set; }

    public bool HasCalculationInputs =>
        BirthDate.HasValue &&
        SexForCalculation.HasValue &&
        HeightCm.HasValue &&
        WeightKg.HasValue &&
        ActivityLevel.HasValue &&
        WeightGoal.HasValue;

    public static UserNutritionProfile CreateManual(
        string userId,
        int dailyCalorieTarget)
    {
        return new UserNutritionProfile(
            userId,
            dailyCalorieTarget);
    }

    public static UserNutritionProfile CreateCalculated(
        string userId,
        int dailyCalorieTarget,
        DateOnly birthDate,
        CalculationSex sexForCalculation,
        decimal heightCm,
        decimal weightKg,
        ActivityLevel activityLevel,
        WeightGoal weightGoal,
        DateOnly currentDate)
    {
        var profile = new UserNutritionProfile(
            userId,
            dailyCalorieTarget);

        profile.UpdateCalculationInputs(
            birthDate,
            sexForCalculation,
            heightCm,
            weightKg,
            activityLevel,
            weightGoal,
            currentDate);

        return profile;
    }

    public void UpdateCalculationInputs(
        DateOnly birthDate,
        CalculationSex sexForCalculation,
        decimal heightCm,
        decimal weightKg,
        ActivityLevel activityLevel,
        WeightGoal weightGoal,
        DateOnly currentDate)
    {
        var validatedBirthDate =
            ValidateBirthDate(birthDate, currentDate);

        var validatedSex = ValidateEnum(
            sexForCalculation,
            nameof(sexForCalculation));

        var validatedHeight = ValidateRange(
            heightCm,
            MinHeightCm,
            MaxHeightCm,
            nameof(heightCm));

        var validatedWeight = ValidateRange(
            weightKg,
            MinWeightKg,
            MaxWeightKg,
            nameof(weightKg));

        var validatedActivityLevel = ValidateEnum(
            activityLevel,
            nameof(activityLevel));

        var validatedWeightGoal = ValidateEnum(
            weightGoal,
            nameof(weightGoal));

        BirthDate = validatedBirthDate;
        SexForCalculation = validatedSex;
        HeightCm = validatedHeight;
        WeightKg = validatedWeight;
        ActivityLevel = validatedActivityLevel;
        WeightGoal = validatedWeightGoal;
    }

    public void ConfirmDailyCalorieTarget(
        int dailyCalorieTarget)
    {
        DailyCalorieTarget =
            ValidateDailyCalorieTarget(dailyCalorieTarget);
    }

    private static string NormalizeUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        return userId.Trim();
    }

    private static DateOnly ValidateBirthDate(
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

        if (age < MinAge || age > MaxAge)
        {
            throw new ArgumentOutOfRangeException(
                nameof(birthDate),
                birthDate,
                $"Age must be between {MinAge} and {MaxAge}.");
        }

        return birthDate;
    }

    private static decimal ValidateRange(
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

        return value;
    }

    private static TEnum ValidateEnum<TEnum>(
        TEnum value,
        string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"{typeof(TEnum).Name} value is invalid.");
        }

        return value;
    }

    private static int ValidateDailyCalorieTarget(
        int dailyCalorieTarget)
    {
        if (
            dailyCalorieTarget < MinDailyCalorieTarget ||
            dailyCalorieTarget > MaxDailyCalorieTarget)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyCalorieTarget),
                dailyCalorieTarget,
                $"Daily calorie target must be between " +
                $"{MinDailyCalorieTarget} and " +
                $"{MaxDailyCalorieTarget} kcal.");
        }

        return dailyCalorieTarget;
    }
}