using Microsoft.EntityFrameworkCore;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Infrastructure.Data;

/// <summary>
/// EF Core database context for sync configuration storage.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>Gets the sync instances.</summary>
    public DbSet<SyncInstance> SyncInstances => Set<SyncInstance>();

    /// <summary>Gets the sync definitions.</summary>
    public DbSet<SyncDefinition> SyncDefinitions => Set<SyncDefinition>();

    /// <summary>Gets the sync target mappings.</summary>
    public DbSet<SyncTargetMapping> SyncTargetMappings => Set<SyncTargetMapping>();

    /// <summary>Gets the sync log entries.</summary>
    public DbSet<SyncLogEntry> SyncLogEntries => Set<SyncLogEntry>();

    /// <summary>Gets the application users.</summary>
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SyncInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InstanceId).IsUnique();
            entity.Property(e => e.InstanceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ConnectionString).IsRequired();
        });

        modelBuilder.Entity<SyncDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SyncId).IsUnique();
            entity.Property(e => e.SyncId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SyncKeyColumns).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IncludeColumns).HasMaxLength(2000);
            entity.Property(e => e.ExcludeColumns).HasMaxLength(2000);
            entity.Property(e => e.SyncUserName).HasMaxLength(100);
            entity.Property(e => e.Topology).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ConflictResolution).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ChangeDetectionMethod).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.RecordFilterInclude).HasMaxLength(4000);
            entity.Property(e => e.RecordFilterExclude).HasMaxLength(4000);
            entity.Property(e => e.AttachmentsStorageType).HasMaxLength(50);

            entity.HasOne(e => e.SourceInstance)
                .WithMany()
                .HasForeignKey(e => e.SourceInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.TargetMappings)
                .WithOne(e => e.SyncDefinition)
                .HasForeignKey(e => e.SyncDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SyncTargetMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetToSourceExcludeColumns).HasMaxLength(2000);
            entity.Property(e => e.TargetExcludeColumns).HasMaxLength(2000);
            entity.Property(e => e.RecordFilterIncludeOverride).HasMaxLength(4000);
            entity.Property(e => e.RecordFilterExcludeOverride).HasMaxLength(4000);

            entity.HasOne(e => e.TargetInstance)
                .WithMany()
                .HasForeignKey(e => e.TargetInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SyncLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SyncId);
            entity.HasIndex(e => e.StartedAt);
            entity.Property(e => e.SyncId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceInstanceId).HasMaxLength(100);
            entity.Property(e => e.TargetInstanceId).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(50);
        });
    }
}
