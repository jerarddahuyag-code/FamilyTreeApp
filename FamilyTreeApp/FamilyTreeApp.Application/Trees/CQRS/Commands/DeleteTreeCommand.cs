using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;

namespace FamilyTreeApp.Application.Trees.CQRS.Commands;

public record DeleteTreeCommand
{
    public required Guid TreeId { get; init; }
}

public class DeleteTreeCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTreeCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteTreeCommand command, CancellationToken cancellationToken = default)
    {
        Tree? tree = await context.Trees
            .FindAsync([command.TreeId], cancellationToken);

        if (tree is null)
        {
            return Result.Failure<bool>(DomainErrors.TreeErrors.TreeNotFound);
        }

        tree.SoftDelete();
        context.Trees.Update(tree);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
