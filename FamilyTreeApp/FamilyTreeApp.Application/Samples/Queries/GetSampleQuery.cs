using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Repositories;

namespace FamilyTreeApp.Application.Samples.Queries;

public record GetSampleQuery(Guid Id);

public record SampleDto(Guid Id, string Name, string Description);

public class GetSampleQueryHandler(ISampleRepository repository) 
    : IQueryHandler<GetSampleQuery, SampleDto>
{
    public async Task<Result<SampleDto>> HandleAsync(GetSampleQuery query, CancellationToken cancellationToken = default)
    {
        var sample = await repository.GetByIdAsync(query.Id, cancellationToken);
        
        if (sample is null)
            return Result.Failure<SampleDto>(new Error("Sample.NotFound", "The sample was not found."));

        return Result.Success(new SampleDto(sample.Id, sample.Name, sample.Description));
    }
}
