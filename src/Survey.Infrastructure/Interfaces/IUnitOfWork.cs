using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Admin> Admins { get; }
    IRepository<SurveyModel> Surveys { get; }
    IRepository<Question> Questions { get; }
    IRepository<QuestionOption> QuestionOptions { get; }
    IRepository<SurveyToken> SurveyTokens { get; }
    IRepository<SurveyResponse> SurveyResponses { get; }
    IRepository<SurveyResponseAnswer> SurveyResponseAnswers { get; }
    IRepository<SurveyResponseAnswerOption> SurveyResponseAnswerOptions { get; }


    /// <summary>
    /// Save all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Get a repository for any entity type
    /// </summary>
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
}