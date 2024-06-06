using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QADraft.Migrations
{
    /// <inheritdoc />
    public partial class AddingTableEditingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isFlagged",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "startDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "endDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isFlagged",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "startDate",
                table: "Users");
        }
    }
}
