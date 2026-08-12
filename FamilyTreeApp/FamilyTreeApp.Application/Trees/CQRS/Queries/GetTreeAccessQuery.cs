using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Domain.Trees.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Trees.CQRS.Queries;

public record GetTreeAccessQuery : IRequest<GetTreeAccessQueryResponse>
{
    public required Guid TreeId { get; init; }
}

public record GetTreeAccessQueryResponse
{
    public required List<GetTreeAccessQueryResponseItem> AccessList { get; init; }
}

public record GetTreeAccessQueryResponseItem
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? AvatarUrl { get; init; }
    public required TreeRole Role { get; init; }
}

public class GetTreeAccessQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTreeAccessQuery, GetTreeAccessQueryResponse>
{
    public async Task<Result<GetTreeAccessQueryResponse>> HandleAsync(GetTreeAccessQuery query, CancellationToken cancellationToken)
    {
        List<TreeRbac> accessList = await context.TreeRbacs
            .Include(tr => tr.User)
            .Where(tr => tr.TreeId == query.TreeId)
            .ToListAsync(cancellationToken);

        var response = new GetTreeAccessQueryResponse
        {
            AccessList = [.. accessList.Select(tr => new GetTreeAccessQueryResponseItem
            {
                UserId = tr.UserId,
                Email = tr.User.Email,
                FirstName = tr.User.ProfileInfo.FirstName,
                LastName = tr.User.ProfileInfo.LastName,
                AvatarUrl = tr.User.ProfileInfo.AvatarUrl,
                Role = tr.TreeRole
            })]
        };

        return Result.Success(response);
    }
}
