using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Boekenlijst.Models;

public partial class BoekenLijstContext : DbContext
{
    public BoekenLijstContext()
    {
    }

    public BoekenLijstContext(DbContextOptions<BoekenLijstContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auteur> Auteurs { get; set; }

    public virtual DbSet<Boek> Boeks { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=BoekenLijst;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.34-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Auteur>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("auteur")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Naam)
                .HasMaxLength(100)
                .HasColumnName("naam");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Voornaam)
                .HasMaxLength(100)
                .HasColumnName("voornaam");
        });

        modelBuilder.Entity<Boek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("boek")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AuteurId, "idx_auteur_id");

            entity.HasIndex(e => e.StatusId, "idx_status_id");

            entity.HasIndex(e => e.Titel, "idx_titel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuteurId).HasColumnName("auteur_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Jaaruitgave).HasColumnName("jaaruitgave");
            entity.Property(e => e.Reeks)
                .HasMaxLength(100)
                .HasColumnName("reeks");
            entity.Property(e => e.ReeksVolgorde).HasColumnName("reeks_volgorde");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Titel).HasColumnName("titel");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Auteur).WithMany(p => p.Boeks)
                .HasForeignKey(d => d.AuteurId)
                .HasConstraintName("boek_ibfk_1");

            entity.HasOne(d => d.Status).WithMany(p => p.Boeks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("boek_ibfk_2");

            entity.HasMany(d => d.Genres).WithMany(p => p.Boeks)
                .UsingEntity<Dictionary<string, object>>(
                    "Boekgenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .HasConstraintName("boekgenre_ibfk_2"),
                    l => l.HasOne<Boek>().WithMany()
                        .HasForeignKey("BoekId")
                        .HasConstraintName("boekgenre_ibfk_1"),
                    j =>
                    {
                        j.HasKey("BoekId", "GenreId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j
                            .ToTable("boekgenre")
                            .UseCollation("utf8mb4_unicode_ci");
                        j.HasIndex(new[] { "GenreId" }, "genre_id");
                        j.IndexerProperty<int>("BoekId").HasColumnName("boek_id");
                        j.IndexerProperty<int>("GenreId").HasColumnName("genre_id");
                    });
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("genre")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Naam, "naam").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Naam)
                .HasMaxLength(100)
                .HasColumnName("naam");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("rating")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.BoekId, "boek_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BoekId).HasColumnName("boek_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Datum)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("datum");
            entity.Property(e => e.Notities)
                .HasColumnType("text")
                .HasColumnName("notities");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Waarde).HasColumnName("waarde");

            entity.HasOne(d => d.Boek).WithOne(p => p.Rating)
                .HasForeignKey<Rating>(d => d.BoekId)
                .HasConstraintName("rating_ibfk_1");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("status")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Naam, "naam").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Naam)
                .HasMaxLength(50)
                .HasColumnName("naam");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
