using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class LocationLongAndLatAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Events",
                newName: "LocationName");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Events",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Events",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "LocationName",
                table: "Events",
                newName: "Location");
        }
    }
}
