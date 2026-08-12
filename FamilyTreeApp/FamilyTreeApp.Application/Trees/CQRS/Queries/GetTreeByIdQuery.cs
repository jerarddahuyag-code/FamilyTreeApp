using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FamilyTreeApp.Application.Trees.CQRS.Queries;

public record GetTreeByIdQuery : IRequest<GetTreeByIdQueryResponse>
{
    public required Guid TreeId { get; init; }
    public ClaimsPrincipal? User { get; init; }
}

public record GetTreeByIdQueryResponse
{
    public required Guid TreeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Role { get; init; }
}

public class GetTreeByIdQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTreeByIdQuery, GetTreeByIdQueryResponse>
{
    public async Task<Result<GetTreeByIdQueryResponse>> HandleAsync(GetTreeByIdQuery query, CancellationToken cancellationToken)
    {
        var userIdStr = query.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _ = Guid.TryParse(userIdStr, out Guid userId);

        Tree? tree = await context.Trees
            .Include(t => t.TreeRbacs)
            .Where(t => t.DeletedAt == null)
            .FirstOrDefaultAsync(t => t.TreeId == query.TreeId, cancellationToken);
        if (tree is null)
        {
            return Result.Failure<GetTreeByIdQueryResponse>(DomainErrors.TreeErrors.TreeNotFound);
        }

        var role = tree.TreeRbacs.FirstOrDefault(r => r.UserId == userId)?.TreeRole.ToString() ?? "None";

        var response = new GetTreeByIdQueryResponse
        {
            TreeId = tree.TreeId,
            Name = tree.Name,
            Description = tree.Description,
            Role = role
        };
        return Result.Success(response);
    }
}
