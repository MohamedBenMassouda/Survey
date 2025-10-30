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

    }
}