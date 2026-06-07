using Microsoft.EntityFrameworkCore.Storage;
using SimpleInventoryManagementSystem.Application.Common.Interfaces;
using SimpleInventoryManagementSystem.Infrastructure.Persistence;

namespace SimpleInventoryManagementSystem.Infrastructure.Services;

public sealed class EfTransactionScopeFactory(SIMSDbContext dbContext) : ITransactionScopeFactory
{
    public async Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionScope(transaction);
    }

    private sealed class EfTransactionScope(IDbContextTransaction dbContextTransaction) : ITransactionScope
    {
        public Task CommitAsync(CancellationToken cancellationToken) => dbContextTransaction.CommitAsync(cancellationToken);
        public Task RollbackAsync(CancellationToken cancellationToken) => dbContextTransaction.RollbackAsync(cancellationToken);
        public ValueTask DisposeAsync() => dbContextTransaction.DisposeAsync();
    }
}
