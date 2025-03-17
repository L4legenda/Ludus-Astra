using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDo_LudusAstra.Migrations
{
    /// <inheritdoc />
    public partial class subtasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubTasks",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTasks",
                table: "Tasks");
        }
    }
}
