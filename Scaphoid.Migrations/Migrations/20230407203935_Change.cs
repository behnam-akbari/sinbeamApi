using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Localization",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    DesignType = table.Column<int>(type: "INTEGER", nullable: false),
                    DesignParameters_GammaG = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_GammaQ = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_ReductionFactorF = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_ModificationFactorKflHtoBLessThanTwo = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_ModificationFactorAllOtherHtoB = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS235LessThan16mm = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS235Between16and40mm = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS235Between40and63mm = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS355LessThan16mm = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS355Between16and40mm = table.Column<double>(type: "REAL", nullable: true),
                    DesignParameters_SteelGradeS355Between40and63mm = table.Column<double>(type: "REAL", nullable: true),
                    DeflectionLimit_VariableLoads = table.Column<double>(type: "REAL", nullable: true),
                    DeflectionLimit_TotalLoads = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localization", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Localization_Orders_OrderId",
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
                name: "Localization");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");
        }
    }
}
