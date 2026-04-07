using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Syncify.Connections.Infrastructure.Persistence.Entities;

namespace Syncify.Connections.Infrastructure.Persistence.Configurations;

internal sealed class CalendarAccountConfiguration : IEntityTypeConfiguration<CalendarAccountEntity>
{
    public void Configure(EntityTypeBuilder<CalendarAccountEntity> builder)
    {
        builder.ToTable("calendar_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .IsRequired();

        builder.Property(x => x.RefreshTokenEnc)
            .HasColumnName("refresh_token_enc")
            .IsRequired();

        builder.Property(x => x.TokenExpiresAt)
            .HasColumnName("token_expires_at")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(x => x.Calendars)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
