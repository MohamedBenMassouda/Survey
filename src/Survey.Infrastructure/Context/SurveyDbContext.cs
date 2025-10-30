using Microsoft.EntityFrameworkCore;

namespace Survey.Infrastructure.Context;

public class SurveyDbContext(DbContextOptions<SurveyDbContext> options) : DbContext(options)
{
    
}