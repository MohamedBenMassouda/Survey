using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Survey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatorIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "surveys",
                newName: "CreatorId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "surveys",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_surveys_CreatorId",
                table: "surveys",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_surveys_admins_CreatorId",
                table: "surveys",
                column: "CreatorId",
                principalTable: "admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_surveys_admins_CreatorId",
                table: "surveys");

            migrationBuilder.DropIndex(
                name: "IX_surveys_CreatorId",
                table: "surveys");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "surveys",
                newName: "CreatedBy");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "surveys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
