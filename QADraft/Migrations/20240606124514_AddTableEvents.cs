using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QADraft.Migrations
{
    /// <inheritdoc />
    public partial class AddTableEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeekQA_Users_CommittedById",
                table: "GeekQA");

            migrationBuilder.DropForeignKey(
                name: "FK_GeekQA_Users_FoundById",
                table: "GeekQA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeekQA",
                table: "GeekQA");

            migrationBuilder.RenameTable(
                name: "GeekQA",
                newName: "GeekQAs");

            migrationBuilder.RenameIndex(
                name: "IX_GeekQA_FoundById",
                table: "GeekQAs",
                newName: "IX_GeekQAs_FoundById");

            migrationBuilder.RenameIndex(
                name: "IX_GeekQA_CommittedById",
                table: "GeekQAs",
                newName: "IX_GeekQAs_CommittedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeekQAs",
                table: "GeekQAs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventInformation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_GeekQAs_Users_CommittedById",
                table: "GeekQAs",
                column: "CommittedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeekQAs_Users_FoundById",
                table: "GeekQAs",
                column: "FoundById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeekQAs_Users_CommittedById",
                table: "GeekQAs");

            migrationBuilder.DropForeignKey(
                name: "FK_GeekQAs_Users_FoundById",
                table: "GeekQAs");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeekQAs",
                table: "GeekQAs");

            migrationBuilder.RenameTable(
                name: "GeekQAs",
                newName: "GeekQA");

            migrationBuilder.RenameIndex(
                name: "IX_GeekQAs_FoundById",
                table: "GeekQA",
                newName: "IX_GeekQA_FoundById");

            migrationBuilder.RenameIndex(
                name: "IX_GeekQAs_CommittedById",
                table: "GeekQA",
                newName: "IX_GeekQA_CommittedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeekQA",
                table: "GeekQA",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GeekQA_Users_CommittedById",
                table: "GeekQA",
                column: "CommittedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GeekQA_Users_FoundById",
                table: "GeekQA",
                column: "FoundById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
