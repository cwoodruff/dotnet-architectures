using Chinook.Clean.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chinook.Clean.Infrastructure.Persistence.Configurations;

/// <summary>Maps the hand-written Domain.Album onto dbo.Album.</summary>
internal sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("Album");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).HasMaxLength(160);
        builder.HasIndex(a => a.ArtistId, "IFK_Artist_Album");
    }
}
