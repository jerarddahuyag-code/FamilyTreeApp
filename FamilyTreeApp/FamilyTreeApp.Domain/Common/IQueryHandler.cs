namespace FamilyTreeApp.Domain.Common;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IRequest<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
