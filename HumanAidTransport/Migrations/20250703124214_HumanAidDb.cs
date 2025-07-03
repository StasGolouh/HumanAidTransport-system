using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanAidTransport.Migrations
{
    /// <inheritdoc />
    public partial class HumanAidDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Debt",
                table: "Volunteers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Debt",
                table: "Volunteers");
        }
    }
}
