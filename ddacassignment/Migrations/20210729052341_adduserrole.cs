using Microsoft.EntityFrameworkCore.Migrations;

namespace ddacassignment.Migrations
{
    public partial class adduserrole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "userrole",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "userrole",
                table: "AspNetUsers");
        }
    }
}
