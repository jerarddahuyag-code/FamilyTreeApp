using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Entities;
using FamilyTreeApp.Domain.Repositories;

namespace FamilyTreeApp.Application.Samples.Commands;

public record CreateSampleCommand(string Name, string Description);

public class CreateSampleCommandHandler(
    ISampleRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateSampleCommand command, CancellationToken cancellationToken = default)
    {
        SampleEntity sample = SampleEntity.Create(command.Name, command.Description);

        await repository.AddAsync(sample, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(sample.Id);
    }
}
