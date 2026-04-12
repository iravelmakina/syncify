using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncify.Sync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncRuleLookbackDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lookback_days",
                table: "sync_rules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lookback_days",
                table: "sync_rules");
        }
    }
}
