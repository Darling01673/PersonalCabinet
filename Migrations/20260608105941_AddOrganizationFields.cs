using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressRegistr",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicantType",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSNILS",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuarantyingSupplier",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationFullName",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationShortName",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportWhoIssued",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentPlan",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SNILS",
                table: "applications",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressRegistr",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ApplicantType",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "DateSNILS",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "GuarantyingSupplier",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "OrganizationFullName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "OrganizationShortName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PassportWhoIssued",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PaymentPlan",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "SNILS",
                table: "applications");
        }
    }
}
