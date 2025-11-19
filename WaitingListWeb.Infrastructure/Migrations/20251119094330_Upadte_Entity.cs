using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitingListWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Upadte_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WaitingListEntries_Email",
                table: "WaitingListEntries");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "WaitingListEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "WaitingListEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "WaitingListEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "WaitingListEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "WaitingListEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WishMessage",
                table: "WaitingListEntries",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "WaitingListEntries");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "WaitingListEntries");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "WaitingListEntries");

            migrationBuilder.DropColumn(
                name: "WishMessage",
                table: "WaitingListEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "WaitingListEntries",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "WaitingListEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitingListEntries_Email",
                table: "WaitingListEntries",
                column: "Email",
                unique: true);
        }
    }
}
