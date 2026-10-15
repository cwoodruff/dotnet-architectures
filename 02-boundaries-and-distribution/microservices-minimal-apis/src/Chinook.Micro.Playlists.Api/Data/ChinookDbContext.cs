using System;
using System.Collections.Generic;
using Chinook.Micro.Playlists.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Micro.Playlists.Api.Data;

public partial class ChinookDbContext : DbContext
{
    public ChinookDbContext(DbContextOptions<ChinookDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistTrack> PlaylistTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Playlist__3214EC07A925B55A");

            entity.ToTable("Playlist");

            entity.HasIndex(e => e.Id, "IPK_Playlist");

            entity.Property(e => e.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<PlaylistTrack>(entity =>
        {
            entity.HasKey(e => new { e.PlaylistId, e.TrackId }).HasName("PK__Playlist__A4A6282E10820D6E");

            entity.ToTable("PlaylistTrack");

            entity.HasIndex(e => e.PlaylistId, "IFK_Playlist_PlaylistTrack");

            entity.HasIndex(e => e.TrackId, "IFK_Track_PlaylistTrack");

            entity.HasIndex(e => e.PlaylistId, "IPK_PlaylistTrack");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistTracks)
                .HasForeignKey(d => d.PlaylistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PlaylistT__Playl__5165187F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
