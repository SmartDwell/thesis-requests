using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Requests.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncidentPointTree",
                table: "Requests",
                newName: "IncidentPointList");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2990),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(2950));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(1380),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(1430));

            migrationBuilder.AddColumn<string>(
                name: "IncidentPointListAsString",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2520),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(2510));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentPointListAsString",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "IncidentPointList",
                table: "Requests",
                newName: "IncidentPointTree");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(2950),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2990));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(1430),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 21, 17, 9, 43, 315, DateTimeKind.Utc).AddTicks(2510),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 1, 22, 16, 56, 16, 379, DateTimeKind.Utc).AddTicks(2520));
        }
    }
}
