using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Users.Entities;

namespace FamilyTreeApp.Application.CQRS.Commands;

public record DeleteUserCommand
{
    public required Guid UserId { get; init; }
}

public class DeleteUserCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUserCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .FindAsync([command.UserId], cancellationToken);
        if (user is null)
        {
            return Result.Failure<bool>(DomainErrors.UserErrors.UserNotFound);
        }

        user.SoftDelete();
        context.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
