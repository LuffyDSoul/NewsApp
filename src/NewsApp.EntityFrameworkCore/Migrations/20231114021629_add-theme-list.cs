using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsApp.Migrations
{
    /// <inheritdoc />
    public partial class addthemelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "AppThemes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppThemes_ThemeId",
                table: "AppThemes",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppThemes_AppThemes_ThemeId",
                table: "AppThemes",
                column: "ThemeId",
                principalTable: "AppThemes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppThemes_AppThemes_ThemeId",
                table: "AppThemes");

            migrationBuilder.DropIndex(
                name: "IX_AppThemes_ThemeId",
                table: "AppThemes");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "AppThemes");
        }
    }
}
