using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Enums;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.CQRS.Queries;

public record GetUserByIdQuery
{
    public required Guid UserId { get; init; }
}

public record GetUserByIdQueryResponse
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

public class GetUserByIdHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse?>
{
    public async Task<Result<GetUserByIdQueryResponse?>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.UserId == query.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<GetUserByIdQueryResponse?>(UserErrors.UserNotFound);

        return Result.Success(new GetUserByIdQueryResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.ProfileInfo.FirstName,
            LastName = user.ProfileInfo.LastName,
            BirthDate = user.ProfileInfo.BirthDate,
            AvatarUrl = user.ProfileInfo.AvatarUrl,
            PhoneNumber = user.ProfileInfo.PhoneNumber,
            Gender = user.ProfileInfo.Gender,
            Bio = user.ProfileInfo.Bio
        })!;
    }
}
