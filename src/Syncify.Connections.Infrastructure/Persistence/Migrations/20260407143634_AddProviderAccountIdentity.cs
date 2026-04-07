using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Syncify.Connections.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderAccountIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "calendar_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "provider_account_id",
                table: "calendar_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_calendar_accounts_user_id_provider_provider_account_id",
                table: "calendar_accounts",
                columns: new[] { "user_id", "provider", "provider_account_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calendar_accounts_user_id_provider_provider_account_id",
                table: "calendar_accounts");

            migrationBuilder.DropColumn(
                name: "email",
                table: "calendar_accounts");

            migrationBuilder.DropColumn(
                name: "provider_account_id",
                table: "calendar_accounts");
        }
    }
}
