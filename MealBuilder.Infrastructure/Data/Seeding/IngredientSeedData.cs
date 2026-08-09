namespace MealBuilder.Infrastructure.Data.Seeding;

internal static class IngredientSeedData
{
    private const string SourceName =
        "Bundeslebensmittelschlüssel (BLS)";

    private const string SourceVersion = "4.0";

    public static object[] Ingredients { get; } =
    [
        Create(
            1, "Chicken breast fillet, raw", "V416100",
            109m, 23.25m, 1.81m, 0m, 0m, 0m, 0.128m),

        Create(
            2, "Ground beef, raw", "U010100",
            224m, 19.102m, 16.39m, 0m, 0m, 0m, 0.145m),

        Create(
            3, "Salmon, deep-frozen", "T410200",
            180m, 19.9m, 11.2m, 0m, 0m, 0m, 0.0798m),

        Create(
            4, "Egg, raw", "E111100",
            135m, 13.175m, 9m, 0.34m, 0.34m, 0m, 0.385m),

        Create(
            5, "Whole milk, fresh, 3.5% fat", "M111300",
            62m, 3.55m, 3.49m, 4.03m, 4.03m, 0m, 0.0872m),

        Create(
            6, "Natural yogurt, 3.5% fat", "M141300",
            67m, 3.98m, 3.46m, 4.13m, 4.13m, 0m, 0.1m),

        Create(
            7, "Quark, low-fat", "M713100",
            66m, 11.85m, 0.18m, 3.68m, 3.68m, 0m, 0.084m),

        Create(
            8, "White rice, raw", "C352000",
            351m, 7.931m, 0.62m, 77.1m, 0.28m, 2.5m, 0.03942m),

        Create(
            9, "Oat flakes", "C133000",
            348m, 13.22m, 6.65m, 53.3m, 0.74m, 10.983m, 0.00495m),

        Create(
            10, "Pasta, egg-free, raw", "E401000",
            346m, 12.4m, 1.6m, 68.833m, 2.015m, 3.406m, 0.0078m),

        Create(
            11, "Wheat bread", "B311000",
            272m, 8.69m, 2.97m, 50.54m, 3.97m, 4m, 1.4205m),

        Create(
            12, "Potato, peeled, raw", "K110100",
            83m, 1.94m, 0.1m, 17.9m, 0.9m, 1.42m, 0.0085m),

        Create(
            13, "Wheat flour, Type 405", "C214100",
            348m, 10.46m, 0.93m, 71.77m, 0.30701m, 5.3m, 0.0022m),

        Create(
            14, "Lentils, mature", "H725100",
            323m, 23.357m, 1.7m, 44.8m, 1.3m, 17.6m, 0.018m),

        Create(
            15, "Olive oil", "Q120000",
            899m, 0m, 99.9m, 0m, 0m, 0m, 0.00372m),

        Create(
            16, "Butter", "Q611000",
            747m, 1.188m, 82.2m, 0.571m, 0.571m, 0m, 0.028m),

        Create(
            17, "Tomato, raw", "G561100",
            22m, 0.95m, 0.11m, 3.25m, 3.25m, 1.3m, 0.01m),

        Create(
            18, "Onion, raw", "G480100",
            34m, 1.156m, 0.15m, 6.01m, 6.01m, 1.4m, 0.022m),

        Create(
            19, "Apple, raw", "F110100",
            58m, 0.424m, 0.5m, 11.7m, 10.487m, 2.275m, 0.002m),

        Create(
            20, "Banana, raw", "F503100",
            79m, 1.319m, 0.4m, 15.89m, 13.89m, 2m, 0.0013m)
    ];

    private static object Create(
        int id,
        string name,
        string sourceCode,
        decimal caloriesPer100g,
        decimal proteinPer100g,
        decimal fatPer100g,
        decimal carbohydratesPer100g,
        decimal sugarsPer100g,
        decimal fiberPer100g,
        decimal saltPer100g)
    {
        return new
        {
            Id = id,
            Name = name,
            CaloriesPer100g = caloriesPer100g,
            ProteinPer100g = proteinPer100g,
            FatPer100g = fatPer100g,
            CarbohydratesPer100g = carbohydratesPer100g,
            SugarsPer100g = sugarsPer100g,
            FiberPer100g = fiberPer100g,
            SaltPer100g = saltPer100g,
            OwnerId = (string?)null,
            SourceName,
            SourceCode = sourceCode,
            SourceVersion
        };
    }
}