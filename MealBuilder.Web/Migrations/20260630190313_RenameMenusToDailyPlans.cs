using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameMenusToDailyPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Menus",
                newName: "DailyPlans");

            migrationBuilder.RenameTable(
                name: "MenuItems",
                newName: "DailyPlanItems");

            migrationBuilder.RenameColumn(
                name: "MenuId",
                table: "DailyPlanItems",
                newName: "DailyPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Menus_Date",
                table: "DailyPlans",
                newName: "IX_DailyPlans_Date");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_MenuId",
                table: "DailyPlanItems",
                newName: "IX_DailyPlanItems_DailyPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_IngredientId",
                table: "DailyPlanItems",
                newName: "IX_DailyPlanItems_IngredientId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_PreparedRecipeBatchId",
                table: "DailyPlanItems",
                newName: "IX_DailyPlanItems_PreparedRecipeBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_RecipeId",
                table: "DailyPlanItems",
                newName: "IX_DailyPlanItems_RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DailyPlans",
                newName: "Menus");

            migrationBuilder.RenameTable(
                name: "DailyPlanItems",
                newName: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "DailyPlanId",
                table: "MenuItems",
                newName: "MenuId");

            migrationBuilder.RenameIndex(
                name: "IX_DailyPlans_Date",
                table: "Menus",
                newName: "IX_Menus_Date");

            migrationBuilder.RenameIndex(
                name: "IX_DailyPlanItems_DailyPlanId",
                table: "MenuItems",
                newName: "IX_MenuItems_MenuId");

            migrationBuilder.RenameIndex(
                name: "IX_DailyPlanItems_IngredientId",
                table: "MenuItems",
                newName: "IX_MenuItems_IngredientId");

            migrationBuilder.RenameIndex(
                name: "IX_DailyPlanItems_PreparedRecipeBatchId",
                table: "MenuItems",
                newName: "IX_MenuItems_PreparedRecipeBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_DailyPlanItems_RecipeId",
                table: "MenuItems",
                newName: "IX_MenuItems_RecipeId");
        }
    }
}
