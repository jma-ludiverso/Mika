using Microsoft.EntityFrameworkCore.Migrations;

namespace MikaWeb.Migrations
{
    public partial class CustomUserData_Empleados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salon",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Salon",
                table: "AspNetUsers");
        }
    }
}
