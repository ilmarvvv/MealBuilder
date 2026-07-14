using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealBuilder.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyPlanOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyPlans_Date",
                table: "DailyPlans");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "DailyPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlans_OwnerId_Date",
                table: "DailyPlans",
                columns: new[] { "OwnerId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyPlans_AspNetUsers_OwnerId",
                table: "DailyPlans",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyPlans_AspNetUsers_OwnerId",
                table: "DailyPlans");

            migrationBuilder.DropIndex(
                name: "IX_DailyPlans_OwnerId_Date",
                table: "DailyPlans");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "DailyPlans");

            migrationBuilder.CreateIndex(
                name: "IX_DailyPlans_Date",
                table: "DailyPlans",
                column: "Date",
                unique: true);
        }
    }
}
