using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class RestrictRecipeDeleteFromPreparedBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreparedRecipeBatches_Recipes_RecipeId",
                table: "PreparedRecipeBatches");

            migrationBuilder.AddForeignKey(
                name: "FK_PreparedRecipeBatches_Recipes_RecipeId",
                table: "PreparedRecipeBatches",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreparedRecipeBatches_Recipes_RecipeId",
                table: "PreparedRecipeBatches");

            migrationBuilder.AddForeignKey(
                name: "FK_PreparedRecipeBatches_Recipes_RecipeId",
                table: "PreparedRecipeBatches",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
