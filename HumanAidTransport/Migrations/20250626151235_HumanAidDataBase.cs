using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanAidTransport.Migrations
{
    /// <inheritdoc />
    public partial class HumanAidDataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "Carriers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CVV",
                table: "Carriers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Carriers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "CVV",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Carriers");
        }
    }
}
