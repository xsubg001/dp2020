using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dochazka.Data.Migrations
{
    public partial class PresenceRecordV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PresenceRecordsV2",
                columns: table => new
                {
                    WorkDay = table.Column<DateTime>(nullable: false),
                    EmployeeId = table.Column<string>(nullable: false),
                    MorningPresence = table.Column<int>(nullable: false),
                    AfternoonPresence = table.Column<int>(nullable: false),
                    ManagerApprovalStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenceRecordsV2", x => new { x.EmployeeId, x.WorkDay });
                    table.ForeignKey(
                        name: "FK_PresenceRecordsV2_AspNetUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresenceRecordsV2");
        }
    }
}
