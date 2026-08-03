using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace FamilyTreeApp.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    ICommandHandler<TRequest, TResponse> innerHandler,
    IApplicationDbContext dbContext)
    : ICommandHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(
        TRequest command,
        CancellationToken cancellationToken = default)
    {
        if (command is not ITransactionalCommand)
        {
            return await innerHandler.HandleAsync(command, cancellationToken);
        }

        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Result<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                await tx.CommitAsync(cancellationToken);
            }
            else
            {
                await tx.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
