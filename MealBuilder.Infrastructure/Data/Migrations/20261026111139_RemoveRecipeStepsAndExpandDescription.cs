using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecipeStepsAndExpandDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeSteps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Instruction = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.CheckConstraint("CK_RecipeSteps_Instruction", "length(trim(Instruction)) BETWEEN 1 AND 2000");
                    table.CheckConstraint("CK_RecipeSteps_Position", "Position > 0");
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_Position",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "Position" });
        }
    }
}
