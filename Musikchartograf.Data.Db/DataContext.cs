using Microsoft.EntityFrameworkCore;
using Musikchartograf.Data.Db.Models;

namespace Musikchartograf.Data.Db;

public class DataContext(DbContextOptions<DataContext> options)
    : DbContext(options)
{
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<PlayedTrack> PlayedTracks => Set<PlayedTrack>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<User> Users => Set<User>();
    public DbSet<YearImport> YearImports => Set<YearImport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entityBuilder =>
        {
            entityBuilder.Property(e => e.Id).IsRequired()
                .ValueGeneratedNever();
            entityBuilder.Property(e => e.Name).IsRequired();

            entityBuilder.HasKey(e => e.Id);
            entityBuilder.HasIndex(e => e.Name).IsUnique();
            entityBuilder.HasMany(e => e.Tracks).WithOne(e => e.Artist)
                .HasForeignKey(e => e.ArtistId)
                .IsRequired();
        });

        modelBuilder.Entity<PlayedTrack>(entityBuilder =>
        {
            entityBuilder.Property(e => e.TrackId).IsRequired();
            entityBuilder.Property(e => e.PlayedByUserName).IsRequired();
            entityBuilder.Property(e => e.PlayedAt).IsRequired();

            entityBuilder.HasKey(e => new
                { e.TrackId, e.PlayedByUserName, e.PlayedAt });
            entityBuilder.HasOne(e => e.Track).WithMany(e => e.Plays)
                .HasForeignKey(e => e.TrackId)
                .IsRequired();
            entityBuilder.HasOne(e => e.PlayedByUser)
                .WithMany(e => e.PlayedTracks)
                .HasForeignKey(e => e.PlayedByUserName)
                .IsRequired();
        });

        modelBuilder.Entity<Track>(entityBuilder =>
        {
            entityBuilder.Property(e => e.Id).IsRequired()
                .ValueGeneratedNever();
            entityBuilder.Property(e => e.Name).IsRequired();
            entityBuilder.Property(e => e.ArtistId).IsRequired();

            entityBuilder.HasKey(e => e.Id);
            entityBuilder.HasOne(e => e.Artist).WithMany(e => e.Tracks)
                .HasForeignKey(e => e.ArtistId)
                .IsRequired();
            entityBuilder.HasMany(e => e.Plays).WithOne(e => e.Track)
                .HasForeignKey(e => e.TrackId).IsRequired();
            entityBuilder.HasIndex(e => new { e.ArtistId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<User>(entityBuilder =>
        {
            entityBuilder.Property(e => e.Name).IsRequired()
                .ValueGeneratedNever();

            entityBuilder.HasKey(e => e.Name);

            entityBuilder.HasMany(e => e.PlayedTracks)
                .WithOne(e => e.PlayedByUser)
                .HasForeignKey(e => e.PlayedByUserName).IsRequired();
        });

        modelBuilder.Entity<YearImport>(entityBuilder =>
        {
            entityBuilder.Property(e => e.Id).IsRequired()
                .ValueGeneratedNever();
            entityBuilder.Property(e => e.UserName).IsRequired();
            entityBuilder.Property(e => e.Year).IsRequired();
            entityBuilder.Property(e => e.Start).IsRequired();
            entityBuilder.Property(e => e.End).IsRequired();

            entityBuilder.HasKey(e => e.Id);
            entityBuilder.HasOne(e => e.User).WithMany(e => e.YearImports)
                .HasForeignKey(e => e.UserName).IsRequired();
            entityBuilder.HasIndex(e => new { e.Year, e.UserName }).IsUnique();
        });
    }
}