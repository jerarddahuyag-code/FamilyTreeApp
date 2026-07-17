using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Enums;
using FamilyTreeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Application.CQRS.Queries;

public record GetUserById
{
    public required Guid UserId { get; init; }
}

public record GetUserByIdResponse
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
    : IQueryHandler<GetUserById, GetUserByIdResponse?>
{
    public async Task<Result<GetUserByIdResponse?>> HandleAsync(GetUserById query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FindAsync([query.UserId], cancellationToken);

        if (user is null)
            return Result.Failure<GetUserByIdResponse?>(new Error("User.NotFound", "The user was not found."));

        return Result.Success(new GetUserByIdResponse
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
