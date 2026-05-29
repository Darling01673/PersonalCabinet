using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPersonalDataAndApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationReason",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DesignDeadline",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceAddress",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnergyDeviceName",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousPowerKw",
                table: "applications",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReliabilityCategory",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPowerKw",
                table: "applications",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPersonalData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ResidenceAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    PassportSeries = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    PassportNumber = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    PassportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPersonalData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPersonalData_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPersonalData_UserId",
                table: "UserPersonalData",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPersonalData");

            migrationBuilder.DropColumn(
                name: "ApplicationReason",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "DesignDeadline",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "DeviceAddress",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "EnergyDeviceName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PreviousPowerKw",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ReliabilityCategory",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "TotalPowerKw",
                table: "applications");
        }
    }
}
