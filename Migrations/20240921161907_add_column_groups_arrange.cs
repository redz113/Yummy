using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFoods.Migrations
{
    /// <inheritdoc />
    public partial class add_column_groups_arrange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Groups_GroupId",
                table: "Menus");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Arrange",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Groups_GroupId",
                table: "Menus",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Groups_GroupId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Arrange",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Menus",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Groups_GroupId",
                table: "Menus",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
