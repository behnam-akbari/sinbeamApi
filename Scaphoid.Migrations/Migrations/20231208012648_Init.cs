using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project = table.Column<string>(type: "TEXT", nullable: true),
                    Designer = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SectionId = table.Column<string>(type: "TEXT", nullable: true),
                    Span = table.Column<double>(type: "REAL", nullable: false),
                    ElementType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Beam",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Span = table.Column<double>(type: "REAL", nullable: false),
                    IsUniformDepth = table.Column<bool>(type: "INTEGER", nullable: false),
                    WebDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    WebDepthRight = table.Column<int>(type: "INTEGER", nullable: false),
                    WebLocalBuckle = table.Column<bool>(type: "INTEGER", nullable: false),
                    WebThickness = table.Column<double>(type: "REAL", nullable: false),
                    WebSteel = table.Column<int>(type: "INTEGER", nullable: false),
                    TopFlangeThickness = table.Column<int>(type: "INTEGER", nullable: false),
                    TopFlangeWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    TopFlangeSteel = table.Column<int>(type: "INTEGER", nullable: false),
                    FixedTopFlange = table.Column<bool>(type: "INTEGER", nullable: false),
                    BottomFlangeThickness = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomFlangeWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    BottomFlangeSteel = table.Column<int>(type: "INTEGER", nullable: false),
                    FixedBottomFlange = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beam", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Beam_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loading",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoadType = table.Column<int>(type: "INTEGER", nullable: false),
                    PermanentLoads_Udl = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_PartialUdl = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_PartialUdlStart = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_PartialUdlEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_EndMomentLeft = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_EndMomentRight = table.Column<int>(type: "INTEGER", nullable: true),
                    PermanentLoads_AxialForce = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_Udl = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_PartialUdl = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_PartialUdlStart = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_PartialUdlEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_EndMomentLeft = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_EndMomentRight = table.Column<int>(type: "INTEGER", nullable: true),
                    VariableLoads_AxialForce = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_Udl = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_PartialUdl = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_PartialUdlStart = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_PartialUdlEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_EndMomentLeft = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_EndMomentRight = table.Column<int>(type: "INTEGER", nullable: true),
                    UltimateLoads_AxialForce = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loading", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Loading_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    DeflectionLimit_TotalLoads = table.Column<double>(type: "REAL", nullable: true),
                    ULSLoadExpression = table.Column<int>(type: "INTEGER", nullable: false),
                    PsiValue = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PointLoad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoadingId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<double>(type: "REAL", nullable: false),
                    Load = table.Column<double>(type: "REAL", nullable: false),
                    PermanentAction = table.Column<double>(type: "REAL", nullable: false),
                    VariableAction = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointLoad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointLoad_Loading_LoadingId",
                        column: x => x.LoadingId,
                        principalTable: "Loading",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointLoad_LoadingId",
                table: "PointLoad",
                column: "LoadingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beam");

            migrationBuilder.DropTable(
                name: "Localization");

            migrationBuilder.DropTable(
                name: "PointLoad");

            migrationBuilder.DropTable(
                name: "Restraint");

            migrationBuilder.DropTable(
                name: "Loading");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
