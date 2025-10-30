using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Survey.Infrastructure.Context;

public class SurveyDbContextFactory : IDesignTimeDbContextFactory<SurveyDbContext>
{
    public SurveyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SurveyDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5437;Database=survey-db;Username=survey;Password=survey;");

        return new SurveyDbContext(optionsBuilder.Options);
    }
}