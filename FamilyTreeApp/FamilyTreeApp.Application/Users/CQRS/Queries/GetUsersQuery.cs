using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.Users.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Users.CQRS.Queries;

public record GetUsersQuery : IRequest<GetUsersQueryResponse>
{
    public required bool IncludePrivate { get; init; }
    public string? SearchEmail { get; init; }
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
        IQueryable<User> queryable = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchEmail))
        {
            queryable = queryable.Where(u => u.Email.ToLower().Contains(query.SearchEmail.ToLower()));
        }

        List<User> users = await queryable
            .Where(u => (query.IncludePrivate || u.IsPublic)
                && u.DeletedAt == null)
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
