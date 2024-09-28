using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFoods.Migrations
{
    /// <inheritdoc />
    public partial class add_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimmOn",
                table: "Tables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComboOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboOrders_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboOrders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuOrders_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuOrders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboOrders_ComboId",
                table: "ComboOrders",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboOrders_TableId",
                table: "ComboOrders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboOrders_Time",
                table: "ComboOrders",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOrders_MenuId",
                table: "MenuOrders",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOrders_TableId",
                table: "MenuOrders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOrders_Time",
                table: "MenuOrders",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboOrders");

            migrationBuilder.DropTable(
                name: "MenuOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "TimmOn",
                table: "Tables");
        }
    }
}
