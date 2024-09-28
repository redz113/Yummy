using Microsoft.EntityFrameworkCore.Migrations;
using AppFoods.Models;
using Bogus;


#nullable disable

namespace AppFoods.Migrations
{
    /// <inheritdoc />
    public partial class updated_column_menu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageSrc",
                table: "Menus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            /*Randomizer.Seed = new Random(546285545);
            var fakeMenu = new Faker<Menu>()
                .RuleFor(m => m.Name, f => f.Name.FullName().ToUpper())
                .RuleFor(m => m.Classify, f => f.Random.ArrayElement<string>(new string[] { "Món Khai Vị", "Món Chính", "Món Phụ", "Món Tráng Miệng" }))
                .RuleFor(m => m.Price, f => f.Random.Number(100000, 10000000))
                .RuleFor(m => m.ImageSrc, f => "/img/menu/noimage.png")
                .RuleFor(m => m.Description, f => f.Lorem.Paragraph());

            for (int i = 0; i < 1000; i++)
            {
                Menu menu = fakeMenu.Generate();       //Tra ve object article

                // Insert data
                migrationBuilder.InsertData(
                    table: "Menus",
                    columns: new[] { "Name", "Classify", "Price", "ImageSrc", "Description" },
                    values: new object[] {
                        menu.Name,
                        menu.Classify, menu.Price, menu.ImageSrc, menu.Description,
                    }
                );
            }*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageSrc",
                table: "Menus");
        }
    }
}
