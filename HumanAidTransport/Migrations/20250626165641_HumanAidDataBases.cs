using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanAidTransport.Migrations
{
    /// <inheritdoc />
    public partial class HumanAidDataBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "Volunteers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CVV",
                table: "Volunteers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Volunteers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "CVV",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Volunteers");
        }
    }
}
