using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Requests.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDefaultDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(9070));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(7400));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(8580));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(9070),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(7400),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RequestComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 2, 28, 9, 11, 16, 964, DateTimeKind.Utc).AddTicks(8580),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
