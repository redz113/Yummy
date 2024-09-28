using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFoods.Migrations
{
    /// <inheritdoc />
    public partial class add_fk_chef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServicePrice",
                table: "Chefs",
                newName: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Chefs_RestaurantId",
                table: "Chefs",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chefs_Restaurant_RestaurantId",
                table: "Chefs",
                column: "RestaurantId",
                principalTable: "Restaurant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chefs_Restaurant_RestaurantId",
                table: "Chefs");

            migrationBuilder.DropIndex(
                name: "IX_Chefs_RestaurantId",
                table: "Chefs");

            migrationBuilder.RenameColumn(
                name: "RestaurantId",
                table: "Chefs",
                newName: "ServicePrice");
        }
    }
}
