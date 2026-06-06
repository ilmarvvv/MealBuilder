using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparedBatchSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipeNameSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCaloriesSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFiberSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalProteinSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSaltSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSugarSnapshot",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipeNameSnapshot",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "TotalCaloriesSnapshot",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "TotalFiberSnapshot",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "TotalProteinSnapshot",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "TotalSaltSnapshot",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "TotalSugarSnapshot",
                table: "PreparedRecipeBatches");
        }
    }
}
