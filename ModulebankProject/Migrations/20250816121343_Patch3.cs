using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModulebankProject.Migrations
{
    /// <inheritdoc />
    public partial class Patch3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Freezing",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Freezing",
                table: "Accounts");
        }
    }
}
