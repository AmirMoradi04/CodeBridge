using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorIdAndFinalPaymentFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "DailyProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "DailyProjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MentorId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_MentorId",
                table: "Contracts",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_MentorId",
                table: "Contracts",
                column: "MentorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_MentorId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_MentorId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "DailyProjects");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "DailyProjects");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Contracts");
        }
    }
}
