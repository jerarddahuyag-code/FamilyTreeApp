using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using System.Security.Claims;

namespace FamilyTreeApp.Application.Trees.CQRS.Commands;

public record CreateTreeCommand : IRequest<Guid>
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required bool IsPublic { get; init; }

    public ClaimsPrincipal? User { get; init; }
}

public class CreateTreeCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTreeCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateTreeCommand command, CancellationToken cancellationToken = default)
    {
        Result<Tree> result = Tree.Create(Guid.NewGuid(), command.Name, command.Description ?? string.Empty, command.IsPublic);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        Tree tree = result.Value;

        await context.Trees.AddAsync(tree, cancellationToken);

        var userIdStr = command.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _ = Guid.TryParse(userIdStr, out Guid ownerId);

        Result<TreeRbac> rbacResult = TreeRbac.Create(Guid.NewGuid(), tree.TreeId, ownerId, Domain.Trees.Enums.TreeRole.Owner);
        if (rbacResult.IsFailure)
        {
            return Result.Failure<Guid>(rbacResult.Error);
        }

        await context.TreeRbacs.AddAsync(rbacResult.Value, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(tree.TreeId);
    }
}
