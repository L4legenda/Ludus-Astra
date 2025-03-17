using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDo_LudusAstra.Migrations
{
    /// <inheritdoc />
    public partial class exptask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "exp",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exp",
                table: "Tasks");
        }
    }
}
