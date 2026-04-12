using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncify.Sync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceStartToSyncedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "source_start",
                table: "synced_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_start",
                table: "synced_events");
        }
    }
}
