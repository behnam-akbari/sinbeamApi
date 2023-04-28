using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddRestraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Restraint",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    FullRestraintTopFlange = table.Column<bool>(type: "INTEGER", nullable: false),
                    TopFlangeRestraints = table.Column<string>(type: "TEXT", nullable: true),
                    FullRestraintBottomFlange = table.Column<bool>(type: "INTEGER", nullable: false),
                    BottomFlangeRestraints = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restraint", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Restraint_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Restraint");
        }
    }
}
