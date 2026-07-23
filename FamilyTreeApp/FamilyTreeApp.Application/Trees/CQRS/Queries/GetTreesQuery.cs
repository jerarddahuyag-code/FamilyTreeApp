using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Trees.CQRS.Queries;

public record GetTreesQuery
{
    public required bool IncludePrivate { get; init; }
}

public record GetTreesQueryResponse
{
    public required List<GetTreesQueryResponseItem> Trees { get; init; }
}

public record GetTreesQueryResponseItem
{
    public required Guid TreeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class GetTreesQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTreesQuery, GetTreesQueryResponse>
{
    public async Task<Result<GetTreesQueryResponse>> HandleAsync(GetTreesQuery query, CancellationToken cancellationToken)
    {
        List<Tree> trees = await context.Trees
            .Where(t => (query.IncludePrivate || t.IsPublic)
                && t.DeletedAt == null)
            .ToListAsync(cancellationToken);
        var response = new GetTreesQueryResponse
        {
            Trees = [.. trees.Select(t => new GetTreesQueryResponseItem
            {
                TreeId = t.TreeId,
                Name = t.Name,
                Description = t.Description
            })]
        };
        return Result.Success(response);
    }
}
