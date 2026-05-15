using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mmotors_back.Migrations
{
    /// <inheritdoc />
    public partial class adjustApplicationServiceentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "ApplicationServices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "ApplicationServices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
