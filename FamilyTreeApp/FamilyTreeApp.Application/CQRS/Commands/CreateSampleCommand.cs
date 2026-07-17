using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Entities;

namespace FamilyTreeApp.Application.Samples.Commands;

public record CreateSampleCommand(string Name, string Description);

public class CreateSampleCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateSampleCommand command, CancellationToken cancellationToken = default)
    {
        SampleEntity sample = SampleEntity.Create(command.Name, command.Description);

        await context.Samples.AddAsync(sample, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(sample.SampleEntityId);
    }
}
