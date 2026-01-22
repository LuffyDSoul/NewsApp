using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsApp.Migrations
{
    /// <inheritdoc />
    public partial class AllowSameArticleInMultipleLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSavedArticles_UserId_Url",
                table: "AppSavedArticles");

            migrationBuilder.CreateIndex(
                name: "IX_AppSavedArticles_UserId_Url_ReadingListId",
                table: "AppSavedArticles",
                columns: new[] { "UserId", "Url", "ReadingListId" },
                unique: true,
                filter: "[ReadingListId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSavedArticles_UserId_Url_ReadingListId",
                table: "AppSavedArticles");

            migrationBuilder.CreateIndex(
                name: "IX_AppSavedArticles_UserId_Url",
                table: "AppSavedArticles",
                columns: new[] { "UserId", "Url" },
                unique: true);
        }
    }
}
