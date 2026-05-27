using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DrawItFasterGame.Migrations
{
    /// <inheritdoc />
    public partial class SeedWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Words",
                columns: new[] { "WordID", "WordString" },
                values: new object[,]
                {
                    { 1, "Apple" },
                    { 2, "Car" },
                    { 3, "Dog" },
                    { 4, "Cat" },
                    { 5, "House" },
                    { 6, "Sun" },
                    { 7, "Tree" },
                    { 8, "Computer" },
                    { 9, "Coffee" },
                    { 10, "Bike" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Words",
                keyColumn: "WordID",
                keyValue: 10);
        }
    }
}
