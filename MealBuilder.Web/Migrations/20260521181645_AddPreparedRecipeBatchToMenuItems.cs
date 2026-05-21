using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparedRecipeBatchToMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreparedRecipeBatchId",
                table: "MenuItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PreparedRecipeBatchId",
                table: "MenuItems",
                column: "PreparedRecipeBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_PreparedRecipeBatches_PreparedRecipeBatchId",
                table: "MenuItems",
                column: "PreparedRecipeBatchId",
                principalTable: "PreparedRecipeBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_PreparedRecipeBatches_PreparedRecipeBatchId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_PreparedRecipeBatchId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "PreparedRecipeBatchId",
                table: "MenuItems");
        }
    }
}
