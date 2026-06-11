using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
     name: "LastName",
     table: "applications",
     nullable: true,
     oldClrType: typeof(string),
     oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "ResidenceAddress",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "PassportSeries",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "PassportNumber",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "PassportWhoIssued",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "AddressRegistr",
                table: "applications",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PassportDate",
                table: "applications",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: false);

            migrationBuilder.AlterColumn<long>(
                name: "SNILS",
                table: "applications",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateSNILS",
                table: "applications",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldNullable: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
