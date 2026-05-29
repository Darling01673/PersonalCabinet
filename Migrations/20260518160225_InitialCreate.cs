using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PersonalCabinet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_types_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'USER'::character varying"),
                    user_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    application_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    object_address = table.Column<string>(type: "text", nullable: false),
                    requested_power = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValueSql: "'NEW'::character varying"),
                    extra_data = table.Column<string>(type: "jsonb", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("applications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "applications_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "individual_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("individual_profiles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "individual_profiles_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    short_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_person = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("organization_profiles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "organization_profiles_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_status_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<long>(type: "bigint", nullable: false),
                    old_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    new_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    changed_by = table.Column<long>(type: "bigint", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("application_status_history_pkey", x => x.id);
                    table.ForeignKey(
                        name: "application_status_history_application_id_fkey",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "application_status_history_changed_by_fkey",
                        column: x => x.changed_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<long>(type: "bigint", nullable: false),
                    document_type_id = table.Column<long>(type: "bigint", nullable: true),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    stored_file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uploaded_by = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("documents_pkey", x => x.id);
                    table.ForeignKey(
                        name: "documents_application_id_fkey",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "documents_document_type_id_fkey",
                        column: x => x.document_type_id,
                        principalTable: "document_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "documents_uploaded_by_fkey",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_id = table.Column<long>(type: "bigint", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("messages_pkey", x => x.id);
                    table.ForeignKey(
                        name: "messages_application_id_fkey",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "messages_sender_id_fkey",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "message_attachments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("message_attachments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "message_attachments_message_id_fkey",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_status_history_application_id",
                table: "application_status_history",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_application_status_history_changed_by",
                table: "application_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "applications_application_number_key",
                table: "applications",
                column: "application_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_applications_user_id",
                table: "applications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "document_types_code_key",
                table: "document_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_documents_application_id",
                table: "documents",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_document_type_id",
                table: "documents",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_uploaded_by",
                table: "documents",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "individual_profiles_user_id_key",
                table: "individual_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_attachments_message_id",
                table: "message_attachments",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_application_id",
                table: "messages",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "organization_profiles_user_id_key",
                table: "organization_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_users_login",
                table: "users",
                column: "login");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_login_key",
                table: "users",
                column: "login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_status_history");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "individual_profiles");

            migrationBuilder.DropTable(
                name: "message_attachments");

            migrationBuilder.DropTable(
                name: "organization_profiles");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
