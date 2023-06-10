using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Requests.Server.Migrations
{
    /// <inheritdoc />
    public partial class RefactoringIncidentPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentPointList",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "IncidentPointListAsString",
                table: "Requests",
                newName: "IncidentPointFullName");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "RequestStatuses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentPointId",
                table: "Requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentPointId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "IncidentPointFullName",
                table: "Requests",
                newName: "IncidentPointListAsString");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "RequestStatuses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "IncidentPointList",
                table: "Requests",
                type: "uuid[]",
                nullable: false);
        }
    }
}
