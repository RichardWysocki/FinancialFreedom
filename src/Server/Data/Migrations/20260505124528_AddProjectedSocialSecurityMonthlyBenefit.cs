using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialFreedom.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectedSocialSecurityMonthlyBenefit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProjectedSocialSecurityMonthlyBenefit",
                table: "RetirementProfiles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectedSocialSecurityMonthlyBenefit",
                table: "RetirementProfiles");
        }
    }
}
