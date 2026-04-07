using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncify.Sync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_calendar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_calendar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    copy_title = table.Column<bool>(type: "boolean", nullable: false),
                    custom_title = table.Column<string>(type: "text", nullable: false),
                    filter_policy = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    sync_cursor = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "synced_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_event_id = table.Column<string>(type: "text", nullable: false),
                    target_block_id = table.Column<string>(type: "text", nullable: false),
                    source_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synced_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_synced_events_sync_rules_sync_rule_id",
                        column: x => x.sync_rule_id,
                        principalTable: "sync_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_synced_events_sync_rule_id_source_event_id",
                table: "synced_events",
                columns: new[] { "sync_rule_id", "source_event_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "synced_events");

            migrationBuilder.DropTable(
                name: "sync_rules");
        }
    }
}
