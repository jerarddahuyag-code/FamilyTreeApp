using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Trees.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Trees.CQRS.Commands;

public record UpdateTreeCommand
{
    public required Guid TreeId { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}

public class UpdateTreeCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTreeCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(UpdateTreeCommand command, CancellationToken cancellationToken = default)
    {
        Tree? existing = await context.Trees
            .Where(t => t.DeletedAt == null)
            .FirstOrDefaultAsync(t => t.TreeId == command.TreeId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure<Guid>(DomainErrors.TreeErrors.TreeNotFound);
        }

        Result result = existing.UpdateDetails(command.Name ?? existing.Name, command.Description ?? existing.Description);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        context.Trees.Update(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(existing.TreeId);
    }
}
