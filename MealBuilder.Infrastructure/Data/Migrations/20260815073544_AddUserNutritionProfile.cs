using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNutritionProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserNutritionProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    DailyCalorieTarget = table.Column<int>(type: "INTEGER", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    SexForCalculation = table.Column<int>(type: "INTEGER", nullable: true),
                    HeightCm = table.Column<double>(type: "REAL", nullable: true),
                    WeightKg = table.Column<double>(type: "REAL", nullable: true),
                    ActivityLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightGoal = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNutritionProfiles", x => x.UserId);
                    table.CheckConstraint("CK_UserNutritionProfiles_ActivityLevel", "ActivityLevel IS NULL OR ActivityLevel IN (1, 2, 3, 4)");
                    table.CheckConstraint("CK_UserNutritionProfiles_CalculationInputs", "(BirthDate IS NULL AND SexForCalculation IS NULL AND HeightCm IS NULL AND WeightKg IS NULL AND ActivityLevel IS NULL AND WeightGoal IS NULL) OR (BirthDate IS NOT NULL AND SexForCalculation IS NOT NULL AND HeightCm IS NOT NULL AND WeightKg IS NOT NULL AND ActivityLevel IS NOT NULL AND WeightGoal IS NOT NULL)");
                    table.CheckConstraint("CK_UserNutritionProfiles_DailyCalorieTarget", "DailyCalorieTarget BETWEEN 1000 AND 10000");
                    table.CheckConstraint("CK_UserNutritionProfiles_HeightCm", "HeightCm IS NULL OR HeightCm BETWEEN 100 AND 250");
                    table.CheckConstraint("CK_UserNutritionProfiles_SexForCalculation", "SexForCalculation IS NULL OR SexForCalculation IN (1, 2)");
                    table.CheckConstraint("CK_UserNutritionProfiles_WeightGoal", "WeightGoal IS NULL OR WeightGoal IN (1, 2, 3)");
                    table.CheckConstraint("CK_UserNutritionProfiles_WeightKg", "WeightKg IS NULL OR WeightKg BETWEEN 30 AND 400");
                    table.ForeignKey(
                        name: "FK_UserNutritionProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNutritionProfiles");
        }
    }
}
