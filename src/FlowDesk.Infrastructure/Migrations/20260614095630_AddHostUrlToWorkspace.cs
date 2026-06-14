using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHostUrlToWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostUrl",
                table: "Workspaces",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostUrl",
                table: "Workspaces");
        }
    }
}
