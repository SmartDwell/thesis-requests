using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Requests.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorContact",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "Requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorContact",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "Requests");
        }
    }
}
