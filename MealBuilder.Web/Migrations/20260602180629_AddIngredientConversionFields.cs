using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientConversionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GramsPerMilliliter",
                table: "Ingredients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GramsPerPiece",
                table: "Ingredients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Ingredients",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GramsPerMilliliter",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "GramsPerPiece",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Ingredients");
        }
    }
}
