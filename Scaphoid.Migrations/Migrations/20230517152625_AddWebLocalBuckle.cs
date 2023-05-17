using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddWebLocalBuckle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WebLocalBuckle",
                table: "Beam",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebLocalBuckle",
                table: "Beam");
        }
    }
}
