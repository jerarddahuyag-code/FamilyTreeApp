using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Trees.CQRS.Queries;

public record GetTreesQuery : IRequest<GetTreesQueryResponse>
{
    public required Guid UserId { get; init; }
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
    public required bool IsPublic { get; init; }
    public required string Role { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public class GetTreesQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTreesQuery, GetTreesQueryResponse>
{
    public async Task<Result<GetTreesQueryResponse>> HandleAsync(GetTreesQuery query, CancellationToken cancellationToken)
    {
        List<GetTreesQueryResponseItem> trees = await context.Trees
            .Where(t => (query.IncludePrivate || t.IsPublic) && t.DeletedAt == null)
            .SelectMany(
                t => t.TreeRbacs.Where(r => r.UserId == query.UserId),
                (t, rbac) => new GetTreesQueryResponseItem
                {
                    TreeId = t.TreeId,
                    Name = t.Name,
                    Description = t.Description,
                    IsPublic = t.IsPublic,
                    Role = rbac.TreeRole.ToString(),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
            .ToListAsync(cancellationToken);

        var response = new GetTreesQueryResponse
        {
            Trees = trees
        };
        return Result.Success(response);
    }
}
