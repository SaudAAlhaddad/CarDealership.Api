using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarDealership.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleIdToOtpToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "OtpTokens",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "OtpTokens");
        }
    }
}
