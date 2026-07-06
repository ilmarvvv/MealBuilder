using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeContentPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "RecipeIngredients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "RecipeComponents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                @"UPDATE RecipeIngredients
                  SET Position = (
                      SELECT COUNT(*)
                      FROM RecipeIngredients AS recipeIngredient
                      WHERE recipeIngredient.RecipeId = RecipeIngredients.RecipeId
                          AND recipeIngredient.Id <= RecipeIngredients.Id
                  );");

            migrationBuilder.Sql(
                @"UPDATE RecipeComponents
                SET Position = (
                    SELECT COUNT(*)
                    FROM RecipeIngredients AS recipeIngredient
                    WHERE recipeIngredient.RecipeId = RecipeComponents.ParentRecipeId
                ) + (
                    SELECT COUNT(*)
                    FROM RecipeComponents AS recipeComponent
                    WHERE recipeComponent.ParentRecipeId = RecipeComponents.ParentRecipeId
                        AND recipeComponent.Id <= RecipeComponents.Id
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "RecipeComponents");
        }
    }
}
