﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Musikchartograf.Data.Db;

#nullable disable

namespace Musikchartograf.Data.Db.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.Artist", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.PlayedTrack", b =>
                {
                    b.Property<Guid>("TrackId")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlayedByUserName")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("PlayedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("TrackId", "PlayedByUserName", "PlayedAt");

                    b.HasIndex("PlayedByUserName");

                    b.ToTable("PlayedTracks");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.Track", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ArtistId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ArtistId", "Name")
                        .IsUnique();

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.User", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.YearImport", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("End")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Start")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserName");

                    b.HasIndex("Year", "UserName")
                        .IsUnique();

                    b.ToTable("YearImports");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.PlayedTrack", b =>
                {
                    b.HasOne("Musikchartograf.Data.Db.Models.User", "PlayedByUser")
                        .WithMany("PlayedTracks")
                        .HasForeignKey("PlayedByUserName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Musikchartograf.Data.Db.Models.Track", "Track")
                        .WithMany("Plays")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlayedByUser");

                    b.Navigation("Track");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.Track", b =>
                {
                    b.HasOne("Musikchartograf.Data.Db.Models.Artist", "Artist")
                        .WithMany("Tracks")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.YearImport", b =>
                {
                    b.HasOne("Musikchartograf.Data.Db.Models.User", "User")
                        .WithMany("YearImports")
                        .HasForeignKey("UserName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.Artist", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.Track", b =>
                {
                    b.Navigation("Plays");
                });

            modelBuilder.Entity("Musikchartograf.Data.Db.Models.User", b =>
                {
                    b.Navigation("PlayedTracks");

                    b.Navigation("YearImports");
                });
#pragma warning restore 612, 618
        }
    }
}
