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
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Volunteers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isBaned",
                table: "Volunteers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Carriers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isBaned",
                table: "Carriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_AdminId",
                table: "Volunteers",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Carriers_AdminId",
                table: "Carriers",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carriers_Admins_AdminId",
                table: "Carriers",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Volunteers_Admins_AdminId",
                table: "Volunteers",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carriers_Admins_AdminId",
                table: "Carriers");

            migrationBuilder.DropForeignKey(
                name: "FK_Volunteers_Admins_AdminId",
                table: "Volunteers");

            migrationBuilder.DropIndex(
                name: "IX_Volunteers_AdminId",
                table: "Volunteers");

            migrationBuilder.DropIndex(
                name: "IX_Carriers_AdminId",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "isBaned",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Carriers");

            migrationBuilder.DropColumn(
                name: "isBaned",
                table: "Carriers");
        }
    }
}
