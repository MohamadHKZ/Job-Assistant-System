using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<EmbeddedProfile> EmbeddedProfiles { get; set; }
    public DbSet<ProfileQualifications> ProfilesQualifications { get; set; }
    public DbSet<JobSource> JobSources { get; set; }
    public DbSet<JobPost> JobPosts { get; set; }
    public DbSet<EmbeddedJobPost> EmbeddedJobPosts { get; set; }
    public DbSet<NormalizedJobPost> NormalizedJobPosts { get; set; }
    public DbSet<Trend> Trends { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Profile>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Profile>(p => p.UserId);

        modelBuilder.Entity<EmbeddedProfile>()
            .HasOne(ep => ep.Profile)
            .WithOne()
            .HasForeignKey<EmbeddedProfile>(ep => ep.ProfileId);

        modelBuilder.Entity<ProfileQualifications>()
            .HasOne(q => q.Profile)
            .WithOne(p => p.ProfileQualifications)
            .HasForeignKey<ProfileQualifications>(q => q.ProfileId);

        // JobPost: single Guid PK + unique composite
        modelBuilder.Entity<JobPost>().HasKey(jp => jp.Id);
        modelBuilder.Entity<JobPost>().HasIndex(jp => new { jp.JobPostId, jp.SourceName }).IsUnique();
        modelBuilder.Entity<JobPost>()
            .HasOne(jp => jp.JobSource)
            .WithMany()
            .HasForeignKey(jp => jp.SourceName);

        // NormalizedJobPost: Guid Id is both PK and FK to JobPost.Id
        modelBuilder.Entity<NormalizedJobPost>().HasKey(njp => njp.Id);
        modelBuilder.Entity<NormalizedJobPost>().HasIndex(njp => new { njp.JobPostId, njp.SourceName }).IsUnique();
        modelBuilder.Entity<NormalizedJobPost>()
            .HasOne(njp => njp.JobPost)
            .WithOne()
            .HasForeignKey<NormalizedJobPost>(njp => njp.Id);

        // EmbeddedJobPost: Guid Id is both PK and FK to NormalizedJobPost.Id
        modelBuilder.Entity<EmbeddedJobPost>().HasKey(ejp => ejp.Id);
        modelBuilder.Entity<EmbeddedJobPost>().HasIndex(ejp => new { ejp.JobPostId, ejp.SourceName }).IsUnique();
        modelBuilder.Entity<EmbeddedJobPost>()
            .HasOne(ejp => ejp.NormalizedJobPost)
            .WithOne()
            .HasForeignKey<EmbeddedJobPost>(ejp => ejp.Id);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);
    }
}
