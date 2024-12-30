using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2e1ee2cd-a6be-4abc-b9e8-fd93adc06de2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5836aa53-8368-414f-af54-48caee6da6f8");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5d5b70b9-f710-44d0-8e6d-11d1148439bd", null, "Admin", "ADMIN" },
                    { "607e60b7-e07b-40ee-ace9-18cc198e576b", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d5b70b9-f710-44d0-8e6d-11d1148439bd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "607e60b7-e07b-40ee-ace9-18cc198e576b");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2e1ee2cd-a6be-4abc-b9e8-fd93adc06de2", null, "User", "USER" },
                    { "5836aa53-8368-414f-af54-48caee6da6f8", null, "Admin", "ADMIN" }
                });
        }
    }
}
