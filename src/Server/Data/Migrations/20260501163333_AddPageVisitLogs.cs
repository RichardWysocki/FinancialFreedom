using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialFreedom.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPageVisitLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageVisitLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PagePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageVisitLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageVisitLogs_VisitedAt",
                table: "PageVisitLogs",
                column: "VisitedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageVisitLogs");
        }
    }
}
