using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garth.Migrations
{
    /// <inheritdoc />
    public partial class AddedEnabledflagtoContexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Contexts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Contexts");
        }
    }
}
