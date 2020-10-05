using Microsoft.EntityFrameworkCore.Migrations;

namespace Dochazka.Migrations
{
    public partial class OptionalTeamInApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_PrimaryManagerId1",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_PrimaryManagerId1",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PrimaryManagerId1",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryManagerId",
                table: "Teams",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_PrimaryManagerId",
                table: "Teams",
                column: "PrimaryManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_PrimaryManagerId",
                table: "Teams",
                column: "PrimaryManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_PrimaryManagerId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_PrimaryManagerId",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryManagerId",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryManagerId1",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_PrimaryManagerId1",
                table: "Teams",
                column: "PrimaryManagerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_PrimaryManagerId1",
                table: "Teams",
                column: "PrimaryManagerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
