using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLoginFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_login",
                table: "users");

            migrationBuilder.DropIndex(
                name: "users_login_key",
                table: "users");

            migrationBuilder.DropColumn(
                name: "login",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "login",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_users_login",
                table: "users",
                column: "login");

            migrationBuilder.CreateIndex(
                name: "users_login_key",
                table: "users",
                column: "login",
                unique: true);
        }
    }
}
