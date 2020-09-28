using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dochazka.Data.Migrations
{
    public partial class ApplicationUserWithCustomTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresenceRecords");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "PresenceRecords",
                columns: table => new
                {
                    employeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayTimeSlot = table.Column<int>(type: "int", nullable: false),
                    ManagerApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    Presence = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenceRecords", x => new { x.employeeId, x.WorkDay, x.DayTimeSlot });
                    table.ForeignKey(
                        name: "FK_PresenceRecords_AspNetUsers_employeeId",
                        column: x => x.employeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
