using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.Users.Enums;

namespace FamilyTreeApp.Application.CQRS.Commands;

public record UpdateProfileCommand
{
    public required Guid UserId { get; init; }

    public bool? IsPublic { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateTime? BirthDate { get; init; }

    public string? AvatarUrl { get; init; }

    public string? PhoneNumber { get; init; }

    public Gender? Gender { get; init; }

    public string? Bio { get; init; }
}

public class UpdateProfileCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProfileCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        User? existing = await context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (existing is null)
            return Result.Failure<Guid>(UserErrors.UserNotFound);

        var current = existing.ProfileInfo ?? new ProfileInfo();

        existing.UpdateProfile(new ProfileInfo
        {
            FirstName = command.FirstName ?? current.FirstName,
            LastName = command.LastName ?? current.LastName,
            BirthDate = command.BirthDate ?? current.BirthDate,
            AvatarUrl = command.AvatarUrl ?? current.AvatarUrl,
            PhoneNumber = command.PhoneNumber ?? current.PhoneNumber,
            Gender = command.Gender ?? current.Gender,
            Bio = command.Bio ?? current.Bio
        });

        if (command.IsPublic.HasValue)
        {
            if (command.IsPublic.Value)
                existing.MakePublic();
            else
                existing.MakePrivate();
        }

        context.Users.Update(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(existing.UserId);
    }
}
