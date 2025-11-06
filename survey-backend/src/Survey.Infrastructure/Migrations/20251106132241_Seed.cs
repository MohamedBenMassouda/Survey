using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "admins",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastModified", "PasswordHash", "UpdatedAt" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"), new DateTime(2025, 11, 6, 13, 13, 7, 709, DateTimeKind.Utc), "admin@example.com", "Admin User", true, null, "$2a$11$aMb7VlbBT6D9UpfZCJUPHOjfoqqtbI2wB70.d/zK.obEPbaQo7GDW", new DateTime(2025, 11, 6, 13, 13, 7, 709, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "admins",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"));
        }
    }
}
