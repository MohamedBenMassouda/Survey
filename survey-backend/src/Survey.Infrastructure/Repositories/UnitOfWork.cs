using Microsoft.EntityFrameworkCore.Storage;
using Survey.Infrastructure.Context;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Infrastructure.Repositories;

public class UnitOfWork(SurveyDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    // Repository instances
    private IRepository<Admin>? _admins;
    private IRepository<SurveyModel>? _surveys;
    private IRepository<Question>? _questions;
    private IRepository<QuestionOption>? _questionOptions;
    private IRepository<SurveyToken>? _surveyTokens;
    private IRepository<SurveyResponse>? _responses;
    private IRepository<SurveyResponseAnswer>? _answers;
    private IRepository<SurveyResponseAnswerOption>? _answerOptions;

    // Cache for dynamically created repositories
    private readonly Dictionary<Type, object> _repositories = new();

    public IRepository<Admin> Admins =>
        _admins ??= new Repository<Admin>(context);

    public IRepository<SurveyModel> Surveys =>
        _surveys ??= new Repository<SurveyModel>(context);

    public IRepository<Question> Questions =>
        _questions ??= new Repository<Question>(context);

    public IRepository<QuestionOption> QuestionOptions =>
        _questionOptions ??= new Repository<QuestionOption>(context);

    public IRepository<SurveyToken> SurveyTokens =>
        _surveyTokens ??= new Repository<SurveyToken>(context);

    public IRepository<SurveyResponse> SurveyResponses =>
        _responses ??= new Repository<SurveyResponse>(context);

    public IRepository<SurveyResponseAnswer> SurveyResponseAnswers =>
        _answers ??= new Repository<SurveyResponseAnswer>(context);

    public IRepository<SurveyResponseAnswerOption> SurveyResponseAnswerOptions =>
        _answerOptions ??= new Repository<SurveyResponseAnswerOption>(context);

    /// <summary>
    /// Get repository for any entity type (with caching)
    /// </summary>
    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity>(context);
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    // ============================================
    // DISPOSE PATTERN
    // ============================================

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                context.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}