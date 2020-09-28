using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dochazka.Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PresenceRecords",
                columns: table => new
                {
                    WorkDay = table.Column<DateTime>(nullable: false),
                    DayTimeSlot = table.Column<int>(nullable: false),
                    employeeId = table.Column<string>(nullable: false),
                    Presence = table.Column<int>(nullable: false),
                    ManagerApprovalStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresenceRecords");
        }
    }
}
