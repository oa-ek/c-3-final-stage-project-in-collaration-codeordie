using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedFixFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "temporary-user-id",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "static-concurrency-stamp", "static-security-stamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "temporary-user-id",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "0426ed37-d3ba-40d3-8d3d-c0e303f3fa93", "50efc77a-6672-4c15-80d2-c26aaf26ab02" });
        }
    }
}
