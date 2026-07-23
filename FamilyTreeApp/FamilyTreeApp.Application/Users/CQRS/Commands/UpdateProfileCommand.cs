using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.Users.Enums;
using FamilyTreeApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Users.CQRS.Commands;

public record UpdateProfileCommand
{
    public Guid UserId { get; init; }

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
        User? user = await context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(DomainErrors.UserErrors.UserNotFound);
        }

        ProfileInfo current = user.ProfileInfo ?? new ProfileInfo();

        Result result = user.UpdateProfile(new ProfileInfo
        {
            FirstName = command.FirstName ?? current.FirstName,
            LastName = command.LastName ?? current.LastName,
            BirthDate = command.BirthDate ?? current.BirthDate,
            AvatarUrl = command.AvatarUrl ?? current.AvatarUrl,
            PhoneNumber = command.PhoneNumber ?? current.PhoneNumber,
            Gender = command.Gender ?? current.Gender,
            Bio = command.Bio ?? current.Bio
        });

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        if (command.IsPublic is true)
        {
            result = user.MakePublic();
        }
        else
        {
            result = user.MakePrivate();
        }

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        context.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.UserId);
    }
}
