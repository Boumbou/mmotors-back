using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mmotors_back.Migrations
{
    /// <inheritdoc />
    public partial class addkeytodocumententity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Documents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Documents");
        }
    }
}
