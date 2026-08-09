using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CaloriesPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    ProteinPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    FatPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    CarbohydratesPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    SugarsPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    FiberPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    SaltPer100g = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.CheckConstraint("CK_Ingredients_CaloriesPer100g", "CaloriesPer100g BETWEEN 0 AND 900");
                    table.CheckConstraint("CK_Ingredients_Name", "length(trim(Name)) BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_Ingredients_NutrientsPer100g", "ProteinPer100g BETWEEN 0 AND 100 AND FatPer100g BETWEEN 0 AND 100 AND CarbohydratesPer100g BETWEEN 0 AND 100 AND SugarsPer100g BETWEEN 0 AND 100 AND FiberPer100g BETWEEN 0 AND 100 AND SaltPer100g BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Ingredients_Ownership", "(OwnerId IS NULL AND SourceName IS NOT NULL AND SourceCode IS NOT NULL AND SourceVersion IS NOT NULL) OR (OwnerId IS NOT NULL AND SourceName IS NULL AND SourceCode IS NULL AND SourceVersion IS NULL)");
                    table.CheckConstraint("CK_Ingredients_Sugars", "SugarsPer100g <= CarbohydratesPer100g");
                    table.ForeignKey(
                        name: "FK_Ingredients_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_OwnerId",
                table: "Ingredients",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingredients");
        }
    }
}
