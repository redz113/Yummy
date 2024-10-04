using AppFoods.Models;
using Bogus;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFoods.Migrations
{
    /// <inheritdoc />
    public partial class add_summary_fake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Randomizer.Seed = new Random(546285545);
            
            var fakeSummary = new Faker<Summary>()
                .RuleFor(m => m.RestaurantId, f => f.Random.Int(1, 4))
                .RuleFor(m => m.TableName, f => f.Random.ClampString("b", 1, 25))
                .RuleFor(m => m.Time, f => f.Date.Between(new DateTime(2021, 1, 1), DateTime.Now))
                .RuleFor(m => m.TotalPrice, f => f.Random.Number(100, 4000));

            for (int i = 0; i < 10000; i++)
            {
                Summary summary = fakeSummary.Generate();       //Tra ve object article

                // Insert data
                migrationBuilder.InsertData(
                    table: "Summarys",
                    columns: new[] { "RestaurantId", "TableName", "Time", "TotalPrice"},
                    values: new object[] {
                        summary.RestaurantId, summary.TableName, summary.Time, summary.TotalPrice
                    }
                );
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
