using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMessageAttachmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "message_attachments_message_id_fkey",
                table: "message_attachments");

            migrationBuilder.DropPrimaryKey(
                name: "message_attachments_pkey",
                table: "message_attachments");

            migrationBuilder.RenameTable(
                name: "message_attachments",
                newName: "MessageAttachment");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MessageAttachment",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "uploaded_at",
                table: "MessageAttachment",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "MessageAttachment",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "file_path",
                table: "MessageAttachment",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "MessageAttachment",
                newName: "FileName");

            migrationBuilder.RenameIndex(
                name: "IX_message_attachments_message_id",
                table: "MessageAttachment",
                newName: "IX_MessageAttachment_MessageId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UploadedAt",
                table: "MessageAttachment",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageAttachment_messages_MessageId",
                table: "MessageAttachment",
                column: "MessageId",
                principalTable: "messages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageAttachment_messages_MessageId",
                table: "MessageAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageAttachment",
                table: "MessageAttachment");

            migrationBuilder.RenameTable(
                name: "MessageAttachment",
                newName: "message_attachments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "message_attachments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "message_attachments",
                newName: "uploaded_at");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "message_attachments",
                newName: "message_id");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "message_attachments",
                newName: "file_path");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "message_attachments",
                newName: "file_name");

            migrationBuilder.RenameIndex(
                name: "IX_MessageAttachment_MessageId",
                table: "message_attachments",
                newName: "IX_message_attachments_message_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "uploaded_at",
                table: "message_attachments",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "message_attachments_pkey",
                table: "message_attachments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "message_attachments_message_id_fkey",
                table: "message_attachments",
                column: "message_id",
                principalTable: "messages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
