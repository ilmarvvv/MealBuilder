using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentRecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComponentRecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Grams = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeComponents_Recipes_ComponentRecipeId",
                        column: x => x.ComponentRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeComponents_Recipes_ParentRecipeId",
                        column: x => x.ParentRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeComponents_ComponentRecipeId",
                table: "RecipeComponents",
                column: "ComponentRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeComponents_ParentRecipeId_ComponentRecipeId",
                table: "RecipeComponents",
                columns: new[] { "ParentRecipeId", "ComponentRecipeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeComponents");
        }
    }
}
