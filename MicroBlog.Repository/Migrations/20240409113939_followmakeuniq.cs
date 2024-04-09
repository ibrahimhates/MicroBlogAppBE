using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroBlog.Repository.Migrations
{
    /// <inheritdoc />
    public partial class followmakeuniq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Followers_UserId",
                table: "Followers");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_UserId_FollowerUserId",
                table: "Followers",
                columns: new[] { "UserId", "FollowerUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Followers_UserId_FollowerUserId",
                table: "Followers");

            migrationBuilder.CreateIndex(
                name: "IX_Followers_UserId",
                table: "Followers",
                column: "UserId");
        }
    }
}
