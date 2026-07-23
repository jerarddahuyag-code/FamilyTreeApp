namespace FamilyTreeApp.Domain.Common;

public interface ICommandHandler<in TCommand, TResult> where TCommand : IRequest<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
