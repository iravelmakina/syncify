using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncify.Notifications.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotificationInboxMinimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notifications_event_type",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_occurred_at",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id_status",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "event_type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "occurred_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "status",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "event_type",
                table: "notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "occurred_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_notifications_event_type",
                table: "notifications",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_occurred_at",
                table: "notifications",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_status",
                table: "notifications",
                columns: new[] { "user_id", "status" });
        }
    }
}
