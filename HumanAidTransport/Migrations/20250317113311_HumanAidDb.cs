using System;
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
            migrationBuilder.CreateTable(
                name: "Carriers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePhotoURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePhotoURL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarrierRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrierId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarrierRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarrierRatings_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HumanitarianAids",
                columns: table => new
                {
                    HumanAidId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payment = table.Column<double>(type: "float", nullable: false),
                    ExpectedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryAddressFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryAddressTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolunteerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarrierId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanitarianAids", x => x.HumanAidId);
                    table.ForeignKey(
                        name: "FK_HumanitarianAids_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HumanitarianAids_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarrierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRequests",
                columns: table => new
                {
                    DeliveryRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarrierId = table.Column<int>(type: "int", nullable: false),
                    CarrierRating = table.Column<int>(type: "int", nullable: true),
                    CarrierContacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HumanAidId = table.Column<int>(type: "int", nullable: false),
                    HumanitarianAidHumanAidId = table.Column<int>(type: "int", nullable: true),
                    HumanAidName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VolunteerId = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequests", x => x.DeliveryRequestId);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_Carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "Carriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_HumanitarianAids_HumanitarianAidHumanAidId",
                        column: x => x.HumanitarianAidHumanAidId,
                        principalTable: "HumanitarianAids",
                        principalColumn: "HumanAidId");
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransportOrders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryRequestId = table.Column<int>(type: "int", nullable: false),
                    HumanAidId = table.Column<int>(type: "int", nullable: false),
                    HumanitarianAidHumanAidId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payment = table.Column<double>(type: "float", nullable: true),
                    DeliveryAddressFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryAddressTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolunteerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportOrders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_TransportOrders_HumanitarianAids_HumanitarianAidHumanAidId",
                        column: x => x.HumanitarianAidHumanAidId,
                        principalTable: "HumanitarianAids",
                        principalColumn: "HumanAidId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarrierRatings_CarrierId",
                table: "CarrierRatings",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_CarrierId",
                table: "DeliveryRequests",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_HumanitarianAidHumanAidId",
                table: "DeliveryRequests",
                column: "HumanitarianAidHumanAidId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_VolunteerId",
                table: "DeliveryRequests",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanitarianAids_CarrierId",
                table: "HumanitarianAids",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanitarianAids_VolunteerId",
                table: "HumanitarianAids",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VolunteerId",
                table: "Notifications",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_HumanitarianAidHumanAidId",
                table: "TransportOrders",
                column: "HumanitarianAidHumanAidId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarrierRatings");

            migrationBuilder.DropTable(
                name: "DeliveryRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TransportOrders");

            migrationBuilder.DropTable(
                name: "HumanitarianAids");

            migrationBuilder.DropTable(
                name: "Carriers");

            migrationBuilder.DropTable(
                name: "Volunteers");
        }
    }
}
