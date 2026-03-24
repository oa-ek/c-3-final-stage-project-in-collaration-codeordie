using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "temporary-user-id", 0, "0426ed37-d3ba-40d3-8d3d-c0e303f3fa93", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "test@travel.com", true, false, null, "TEST@TRAVEL.COM", "TEST@TRAVEL.COM", null, null, false, "50efc77a-6672-4c15-80d2-c26aaf26ab02", false, "test@travel.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "temporary-user-id");
        }
    }
}
