using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IncludeInWeeklySummary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPlans_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparedRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    SourceRecipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    NameSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PreparedDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TotalPortions = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparedRecipes", x => x.Id);
                    table.CheckConstraint("CK_PreparedRecipes_NameSnapshot", "length(trim(NameSnapshot)) BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_PreparedRecipes_TotalPortions", "TotalPortions > 0");
                    table.ForeignKey(
                        name: "FK_PreparedRecipes_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparedRecipes_Recipes_SourceRecipeId",
                        column: x => x.SourceRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DailyPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<int>(type: "INTEGER", nullable: true),
                    PreparedRecipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Grams = table.Column<double>(type: "REAL", nullable: true),
                    Portions = table.Column<double>(type: "REAL", nullable: true),
                    PlannedTime = table.Column<TimeOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPlanItems", x => x.Id);
                    table.CheckConstraint("CK_DailyPlanItems_Grams", "Grams IS NULL OR (Grams > 0 AND Grams <= 100000)");
                    table.CheckConstraint("CK_DailyPlanItems_ItemType", "ItemType IN (1, 2)");
                    table.CheckConstraint("CK_DailyPlanItems_Portions", "Portions IS NULL OR (Portions > 0 AND Portions = ROUND(Portions, 2))");
                    table.CheckConstraint("CK_DailyPlanItems_SourceAndQuantity", "(ItemType = 1 AND IngredientId IS NOT NULL AND PreparedRecipeId IS NULL AND Grams IS NOT NULL AND Portions IS NULL) OR (ItemType = 2 AND IngredientId IS NULL AND PreparedRecipeId IS NOT NULL AND Grams IS NULL AND Portions IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_DailyPlanItems_DailyPlans_DailyPlanId",
                        column: x => x.DailyPlanId,
                        principalTable: "DailyPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyPlanItems_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyPlanItems_PreparedRecipes_PreparedRecipeId",
                        column: x => x.PreparedRecipeId,
                        principalTable: "PreparedRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparedRecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PreparedRecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    NameSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Grams = table.Column<double>(type: "REAL", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Calories = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Protein = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Fat = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Carbohydrates = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Sugars = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Fiber = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    Salt = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparedRecipeIngredients", x => x.Id);
                    table.CheckConstraint("CK_PreparedRecipeIngredients_Grams", "Grams > 0 AND Grams <= 100000");
                    table.CheckConstraint("CK_PreparedRecipeIngredients_NameSnapshot", "length(trim(NameSnapshot)) BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_PreparedRecipeIngredients_Nutrition", "Calories >= 0 AND Protein >= 0 AND Fat >= 0 AND Carbohydrates >= 0 AND Sugars >= 0 AND Fiber >= 0 AND Salt >= 0");
                    table.CheckConstraint("CK_PreparedRecipeIngredients_Position", "Position > 0");
                    table.CheckConstraint("CK_PreparedRecipeIngredients_Sugars", "Sugars <= Carbohydrates");
                    table.ForeignKey(
                        name: "FK_PreparedRecipeIngredients_PreparedRecipes_PreparedRecipeId",
                        column: x => x.PreparedRecipeId,
                        principalTable: "PreparedRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanItems_DailyPlanId_PlannedTime_Id",
                table: "DailyPlanItems",
                columns: new[] { "DailyPlanId", "PlannedTime", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanItems_IngredientId",
                table: "DailyPlanItems",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlanItems_PreparedRecipeId",
                table: "DailyPlanItems",
                column: "PreparedRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlans_OwnerId_Date",
                table: "DailyPlans",
                columns: new[] { "OwnerId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeIngredients_PreparedRecipeId_Position",
                table: "PreparedRecipeIngredients",
                columns: new[] { "PreparedRecipeId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipes_OwnerId_NameSnapshot",
                table: "PreparedRecipes",
                columns: new[] { "OwnerId", "NameSnapshot" });

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipes_OwnerId_PreparedDate",
                table: "PreparedRecipes",
                columns: new[] { "OwnerId", "PreparedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipes_SourceRecipeId",
                table: "PreparedRecipes",
                column: "SourceRecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyPlanItems");

            migrationBuilder.DropTable(
                name: "PreparedRecipeIngredients");

            migrationBuilder.DropTable(
                name: "DailyPlans");

            migrationBuilder.DropTable(
                name: "PreparedRecipes");
        }
    }
}
