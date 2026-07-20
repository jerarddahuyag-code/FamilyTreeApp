using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Samples.Commands;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Enums;
using FamilyTreeApp.Domain.Entities;
using FamilyTreeApp.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Application.CQRS.Commands;

public record CreateUserCommand
{
    public required string Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateTime? BirthDate { get; init; }

    public string? AvatarUrl { get; init; }

    public string? PhoneNumber { get; init; }

    public Gender? Gender { get; init; }

    public string? Bio { get; init; }

    public required bool IsPublic { get; init; }
}

public class CreateUserCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = command.Email,
            IsPublic = command.IsPublic,
            ProfileInfo = new ProfileInfo
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                BirthDate = command.BirthDate,
                AvatarUrl = command.AvatarUrl,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                Bio = command.Bio
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await context.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.UserId);
    }
}
