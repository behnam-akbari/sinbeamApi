using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPsi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PsiValue",
                table: "Localization",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ULSLoadExpression",
                table: "Localization",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PsiValue",
                table: "Localization");

            migrationBuilder.DropColumn(
                name: "ULSLoadExpression",
                table: "Localization");
        }
    }
}
