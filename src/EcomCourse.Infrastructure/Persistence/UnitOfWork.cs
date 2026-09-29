using EcomCourse.Application.Abstractions;

namespace EcomCourse.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EcomCourseDbContext _context;

    public UnitOfWork(EcomCourseDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = _context.Database.CurrentTransaction;

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = _context.Database.CurrentTransaction;

        if (transaction is not null)
            await transaction.RollbackAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
