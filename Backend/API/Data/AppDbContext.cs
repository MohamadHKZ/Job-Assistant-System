using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Profile>(p => p.UserId);

        modelBuilder.Entity<EmbeddedProfile>()
            .HasOne(ep => ep.Profile)
            .WithOne()
            .HasForeignKey<EmbeddedProfile>(ep => ep.ProfileId);

        modelBuilder.Entity<EmbeddedJobPost>()
            .HasOne(ejp => ejp.NormalizedJobPost)
            .WithOne()
            .HasForeignKey<EmbeddedJobPost>(ejp => ejp.JobPostId);

        modelBuilder.Entity<NormalizedJobPost>()
            .HasOne(njp => njp.JobPost)
            .WithOne()
            .HasForeignKey<NormalizedJobPost>(njp => njp.JobPostId);

        modelBuilder.Entity<ProfileQualifications>()
            .HasOne(q => q.Profile)
            .WithOne(p => p.ProfileQualifications)
            .HasForeignKey<ProfileQualifications>(q => q.ProfileId);
        modelBuilder.Entity<JobPost>()
            .HasKey(jp => new { jp.JobPostId, jp.SourceName });
        modelBuilder.Entity<JobPost>()
            .HasOne(jp => jp.JobSource)
            .WithMany()
            .HasForeignKey(jp => jp.SourceName);

        modelBuilder.Entity<EmbeddedJobPost>()
            .HasKey(ejp => new { ejp.JobPostId, ejp.SourceName });
        modelBuilder.Entity<EmbeddedJobPost>()
            .HasOne(ejp => ejp.NormalizedJobPost)
            .WithOne()
            .HasForeignKey<EmbeddedJobPost>(ejp => new { ejp.JobPostId, ejp.SourceName });

        modelBuilder.Entity<NormalizedJobPost>()
            .HasKey(njp => new { njp.JobPostId, njp.SourceName });
        modelBuilder.Entity<NormalizedJobPost>()
            .HasOne(njp => njp.JobPost)
            .WithOne()
            .HasForeignKey<NormalizedJobPost>(njp => new { njp.JobPostId, njp.SourceName });
    }
}
