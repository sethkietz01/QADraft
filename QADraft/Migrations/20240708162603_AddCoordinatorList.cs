using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QADraft.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatorList : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "theme",
                table: "Users");
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "theme",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
 
    }
}
