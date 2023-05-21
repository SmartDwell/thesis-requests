using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Requests.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(9070),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2990));

            migrationBuilder.AddColumn<string>(
                name: "CreatorName",
                table: "RequestStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(7400),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.AddColumn<string>(
                name: "CreatorName",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(8580),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2520));

            migrationBuilder.AddColumn<string>(
                name: "CreatorName",
                table: "RequestComments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorName",
                table: "RequestStatuses");

            migrationBuilder.DropColumn(
                name: "CreatorName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CreatorName",
                table: "RequestComments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2990),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(9070));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(1380),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(7400));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2520),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(8580));
        }
    }
}
