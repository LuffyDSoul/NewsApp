using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsApp.Migrations
{
    /// <inheritdoc />
    public partial class addusertotheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AppThemes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppThemes_UserId",
                table: "AppThemes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppThemes_AbpUsers_UserId",
                table: "AppThemes",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppThemes_AbpUsers_UserId",
                table: "AppThemes");

            migrationBuilder.DropIndex(
                name: "IX_AppThemes_UserId",
                table: "AppThemes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AppThemes");
        }
    }
}
