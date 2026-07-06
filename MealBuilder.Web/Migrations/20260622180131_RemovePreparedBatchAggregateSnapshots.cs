using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreparedBatchAggregateSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
