using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroBlog.Repository.Migrations
{
    /// <inheritdoc />
    public partial class modeleditmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerifyToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VerifyEmail",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerifyToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerifyEmail",
                table: "Users");
        }
    }
}
