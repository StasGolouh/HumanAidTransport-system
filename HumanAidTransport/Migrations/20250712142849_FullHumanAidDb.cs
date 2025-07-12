using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanAidTransport.Migrations
{
    /// <inheritdoc />
    public partial class FullHumanAidDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "Volunteers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "Volunteers");
        }
    }
}
