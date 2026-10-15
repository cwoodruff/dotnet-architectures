using Chinook.Clean.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chinook.Clean.Infrastructure.Persistence.Configurations;

/// <summary>Maps the hand-written Domain.Artist onto dbo.Artist.</summary>
internal sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("Artist");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(120);

        builder.HasMany(a => a.Albums)
            .WithOne()
            .HasForeignKey(al => al.ArtistId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Albums is a read-only view over a private list; EF reads and writes the field.
        builder.Navigation(a => a.Albums).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
