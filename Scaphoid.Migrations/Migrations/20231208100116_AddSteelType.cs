using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSteelType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SteelType",
                table: "Localization",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SteelType",
                table: "Localization");
        }
    }
}
