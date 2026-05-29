using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Inn",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportDate",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PassportSeries",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidenceAddress",
                table: "applications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "Inn",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PassportDate",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "PassportSeries",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ResidenceAddress",
                table: "applications");
        }
    }
}
