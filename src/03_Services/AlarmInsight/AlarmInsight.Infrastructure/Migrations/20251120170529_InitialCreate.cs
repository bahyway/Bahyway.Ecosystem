using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlarmInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alarms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    location_latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    location_longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    location_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    severity_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity_value = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alarm_notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alarm_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_alarm_notes_alarms_alarm_id",
                        column: x => x.alarm_id,
                        principalTable: "alarms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alarm_notes_alarm_id",
                table: "alarm_notes",
                column: "alarm_id");

            migrationBuilder.CreateIndex(
                name: "ix_alarms_occurred_at",
                table: "alarms",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_alarms_status",
                table: "alarms",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_alarms_status_occurred_at",
                table: "alarms",
                columns: new[] { "status", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_notes");

            migrationBuilder.DropTable(
                name: "alarms");
        }
    }
}
