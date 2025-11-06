using Microsoft.EntityFrameworkCore;
using Survey.Infrastructure.Enums;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Context;

// TODO: Add permissions for admins
public class SurveyDbContext(DbContextOptions<SurveyDbContext> options) : DbContext(options)
{
    public DbSet<SurveyModel> Surveys { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<SurveyToken> SurveyTokens { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyResponseAnswer> SurveyResponseAnswers { get; set; }
    public DbSet<SurveyResponseAnswerOption> SurveyResponseAnswerOptions { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Question>()
            .Property(entity => entity.Type)
            .HasConversion<string>();

        modelBuilder
            .Entity<SurveyModel>()
            .Property(entity => entity.Status)
            .HasConversion<string>();

        OnModelSeeding(modelBuilder);
    }

    private void OnModelSeeding(ModelBuilder modelBuilder)
    {
        var admin = new Admin
        {
            Id = new Guid("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
            FullName = "Admin User",
            Email = "admin@example.com",
            PasswordHash = "$2a$11$aMb7VlbBT6D9UpfZCJUPHOjfoqqtbI2wB70.d/zK.obEPbaQo7GDW",
            IsActive = true,
            CreatedAt = new DateTime(2025, 11, 6, 13, 13, 7, 709, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 11, 6, 13, 13, 7, 709, DateTimeKind.Utc)
        };

        modelBuilder.Entity<Admin>().HasData(admin);
    }
}