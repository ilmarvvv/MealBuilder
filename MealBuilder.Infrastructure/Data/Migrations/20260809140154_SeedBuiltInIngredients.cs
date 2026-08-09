using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MealBuilder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedBuiltInIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "FatPer100g", "Name", "OwnerId", "ProteinPer100g", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion" },
                values: new object[,]
                {
                    { 1, 109.0, 1.8100000000000001, "Chicken breast fillet, raw", null, 23.25, 0.128, "V416100", "Bundeslebensmittelschlüssel (BLS)", "4.0" },
                    { 2, 224.0, 16.390000000000001, "Ground beef, raw", null, 19.102, 0.14499999999999999, "U010100", "Bundeslebensmittelschlüssel (BLS)", "4.0" },
                    { 3, 180.0, 11.199999999999999, "Salmon, deep-frozen", null, 19.899999999999999, 0.079799999999999996, "T410200", "Bundeslebensmittelschlüssel (BLS)", "4.0" }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "CarbohydratesPer100g", "FatPer100g", "Name", "OwnerId", "ProteinPer100g", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion", "SugarsPer100g" },
                values: new object[,]
                {
                    { 4, 135.0, 0.34000000000000002, 9.0, "Egg, raw", null, 13.175000000000001, 0.38500000000000001, "E111100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.34000000000000002 },
                    { 5, 62.0, 4.0300000000000002, 3.4900000000000002, "Whole milk, fresh, 3.5% fat", null, 3.5499999999999998, 0.0872, "M111300", "Bundeslebensmittelschlüssel (BLS)", "4.0", 4.0300000000000002 },
                    { 6, 67.0, 4.1299999999999999, 3.46, "Natural yogurt, 3.5% fat", null, 3.98, 0.10000000000000001, "M141300", "Bundeslebensmittelschlüssel (BLS)", "4.0", 4.1299999999999999 },
                    { 7, 66.0, 3.6800000000000002, 0.17999999999999999, "Quark, low-fat", null, 11.85, 0.084000000000000005, "M713100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 3.6800000000000002 }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "CarbohydratesPer100g", "FatPer100g", "FiberPer100g", "Name", "OwnerId", "ProteinPer100g", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion", "SugarsPer100g" },
                values: new object[,]
                {
                    { 8, 351.0, 77.099999999999994, 0.62, 2.5, "White rice, raw", null, 7.931, 0.039419999999999997, "C352000", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.28000000000000003 },
                    { 9, 348.0, 53.299999999999997, 6.6500000000000004, 10.983000000000001, "Oat flakes", null, 13.220000000000001, 0.0049500000000000004, "C133000", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.73999999999999999 },
                    { 10, 346.0, 68.832999999999998, 1.6000000000000001, 3.4060000000000001, "Pasta, egg-free, raw", null, 12.4, 0.0077999999999999996, "E401000", "Bundeslebensmittelschlüssel (BLS)", "4.0", 2.0150000000000001 },
                    { 11, 272.0, 50.539999999999999, 2.9700000000000002, 4.0, "Wheat bread", null, 8.6899999999999995, 1.4205000000000001, "B311000", "Bundeslebensmittelschlüssel (BLS)", "4.0", 3.9700000000000002 },
                    { 12, 83.0, 17.899999999999999, 0.10000000000000001, 1.4199999999999999, "Potato, peeled, raw", null, 1.9399999999999999, 0.0085000000000000006, "K110100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.90000000000000002 },
                    { 13, 348.0, 71.769999999999996, 0.93000000000000005, 5.2999999999999998, "Wheat flour, Type 405", null, 10.460000000000001, 0.0022000000000000001, "C214100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.30701000000000001 },
                    { 14, 323.0, 44.799999999999997, 1.7, 17.600000000000001, "Lentils, mature", null, 23.356999999999999, 0.017999999999999999, "H725100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 1.3 }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "FatPer100g", "Name", "OwnerId", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion" },
                values: new object[] { 15, 899.0, 99.900000000000006, "Olive oil", null, 0.0037200000000000002, "Q120000", "Bundeslebensmittelschlüssel (BLS)", "4.0" });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "CarbohydratesPer100g", "FatPer100g", "Name", "OwnerId", "ProteinPer100g", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion", "SugarsPer100g" },
                values: new object[] { 16, 747.0, 0.57099999999999995, 82.200000000000003, "Butter", null, 1.1879999999999999, 0.028000000000000001, "Q611000", "Bundeslebensmittelschlüssel (BLS)", "4.0", 0.57099999999999995 });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "CarbohydratesPer100g", "FatPer100g", "FiberPer100g", "Name", "OwnerId", "ProteinPer100g", "SaltPer100g", "SourceCode", "SourceName", "SourceVersion", "SugarsPer100g" },
                values: new object[,]
                {
                    { 17, 22.0, 3.25, 0.11, 1.3, "Tomato, raw", null, 0.94999999999999996, 0.01, "G561100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 3.25 },
                    { 18, 34.0, 6.0099999999999998, 0.14999999999999999, 1.3999999999999999, "Onion, raw", null, 1.1559999999999999, 0.021999999999999999, "G480100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 6.0099999999999998 },
                    { 19, 58.0, 11.699999999999999, 0.5, 2.2749999999999999, "Apple, raw", null, 0.42399999999999999, 0.002, "F110100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 10.487 },
                    { 20, 79.0, 15.890000000000001, 0.40000000000000002, 2.0, "Banana, raw", null, 1.319, 0.0012999999999999999, "F503100", "Bundeslebensmittelschlüssel (BLS)", "4.0", 13.890000000000001 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
