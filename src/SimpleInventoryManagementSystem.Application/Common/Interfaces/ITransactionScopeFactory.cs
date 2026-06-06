namespace SimpleInventoryManagementSystem.Application.Common.Interfaces;

public interface ITransactionScopeFactory
{
    Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken = default);
}
