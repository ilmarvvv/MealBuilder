using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class MoveRecipeDaysToPreparedBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Days",
                table: "Recipes");

            migrationBuilder.AddColumn<int>(
                name: "PlannedDays",
                table: "PreparedRecipeBatches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDays",
                table: "PreparedRecipeBatches");

            migrationBuilder.AddColumn<int>(
                name: "Days",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }
    }
}
