using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparedRecipeBatchItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreparedRecipeBatchItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PreparedRecipeBatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceIngredientId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceRecipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    NameSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Grams = table.Column<decimal>(type: "TEXT", nullable: false),
                    CaloriesSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProteinSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    FiberSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    SugarSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    SaltSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparedRecipeBatchItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparedRecipeBatchItems_Ingredients_SourceIngredientId",
                        column: x => x.SourceIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparedRecipeBatchItems_PreparedRecipeBatches_PreparedRecipeBatchId",
                        column: x => x.PreparedRecipeBatchId,
                        principalTable: "PreparedRecipeBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreparedRecipeBatchItems_Recipes_SourceRecipeId",
                        column: x => x.SourceRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeBatchItems_PreparedRecipeBatchId",
                table: "PreparedRecipeBatchItems",
                column: "PreparedRecipeBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeBatchItems_SourceIngredientId",
                table: "PreparedRecipeBatchItems",
                column: "SourceIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeBatchItems_SourceRecipeId",
                table: "PreparedRecipeBatchItems",
                column: "SourceRecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreparedRecipeBatchItems");
        }
    }
}
