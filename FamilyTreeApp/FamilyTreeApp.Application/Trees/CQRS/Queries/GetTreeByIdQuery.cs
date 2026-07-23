using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Trees.CQRS.Queries;

public record GetTreeByIdQuery : IRequest<GetTreeByIdQueryResponse>
{
    public required Guid TreeId { get; init; }
}

public record GetTreeByIdQueryResponse
{
    public required Guid TreeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class GetTreeByIdQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTreeByIdQuery, GetTreeByIdQueryResponse>
{
    public async Task<Result<GetTreeByIdQueryResponse>> HandleAsync(GetTreeByIdQuery query, CancellationToken cancellationToken)
    {
        Tree? tree = await context.Trees
            .Where(t => t.DeletedAt == null)
            .FirstOrDefaultAsync(t => t.TreeId == query.TreeId, cancellationToken);
        if (tree is null)
        {
            return Result.Failure<GetTreeByIdQueryResponse>(DomainErrors.TreeErrors.TreeNotFound);
        }

        var response = new GetTreeByIdQueryResponse
        {
            TreeId = tree.TreeId,
            Name = tree.Name,
            Description = tree.Description
        };
        return Result.Success(response);
    }
}
