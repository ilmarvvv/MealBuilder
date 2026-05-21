using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparedRecipeBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreparedRecipeBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CookedDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TotalServings = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparedRecipeBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparedRecipeBatches_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeBatches_RecipeId",
                table: "PreparedRecipeBatches",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreparedRecipeBatches");
        }
    }
}
