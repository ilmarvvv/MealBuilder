using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparedRecipeBatchOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "PreparedRecipeBatches",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PreparedRecipeBatches_OwnerId",
                table: "PreparedRecipeBatches",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreparedRecipeBatches_AspNetUsers_OwnerId",
                table: "PreparedRecipeBatches",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreparedRecipeBatches_AspNetUsers_OwnerId",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropIndex(
                name: "IX_PreparedRecipeBatches_OwnerId",
                table: "PreparedRecipeBatches");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "PreparedRecipeBatches");
        }
    }
}
