using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.Users.Enums;
using FamilyTreeApp.Domain.ValueObjects;

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
        var result = User.Create(
            Guid.NewGuid(),
            command.Email,
            new ProfileInfo
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                BirthDate = command.BirthDate,
                AvatarUrl = command.AvatarUrl,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                Bio = command.Bio
            });

        if (!result.IsSuccess)
        {
            return Result.Failure<Guid>(result.Error);
        }

        User user = result.Value;

        await context.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.UserId);
    }
}
