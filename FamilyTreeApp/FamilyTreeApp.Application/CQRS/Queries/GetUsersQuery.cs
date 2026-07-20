using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Enums;
using FamilyTreeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.CQRS.Queries;

public record GetUsersQuery
{
    public required bool IncludePrivate { get; init; }
}

public record GetUsersQueryResponse
{
    public required List<GetUsersQueryResponseItem> UserList { get; init; }
}

public record GetUsersQueryResponseItem
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateTime? BirthDate { get; init; }

    public string? AvatarUrl { get; init; }

    public string? PhoneNumber { get; init; }

    public Gender? Gender { get; init; }

    public string? Bio { get; init; }
}

public class GetUsersQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetUsersQuery, GetUsersQueryResponse>
{
    public async Task<Result<GetUsersQueryResponse>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        List<User> users = await context.Users
            .Where(u => query.IncludePrivate || u.IsPublic)
            .ToListAsync(cancellationToken);

        var response = new GetUsersQueryResponse
        {
            UserList = [.. users.Select(u => new GetUsersQueryResponseItem
            {
                UserId = u.UserId,
                Email = u.Email,
                FirstName = u.ProfileInfo.FirstName,
                LastName = u.ProfileInfo.LastName,
                BirthDate = u.ProfileInfo.BirthDate,
                AvatarUrl = u.ProfileInfo.AvatarUrl,
                PhoneNumber = u.ProfileInfo.PhoneNumber,
                Gender = u.ProfileInfo.Gender,
                Bio = u.ProfileInfo.Bio
            })]
        };

        return Result.Success(response);
    }
}
