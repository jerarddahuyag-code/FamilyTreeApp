using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Entities;

namespace FamilyTreeApp.Application.Samples.Queries;

public record GetSampleQuery(Guid Id);

public record SampleDto(Guid Id, string Name, string Description);

public class GetSampleQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSampleQuery, SampleDto>
{
    public async Task<Result<SampleDto>> HandleAsync(GetSampleQuery query, CancellationToken cancellationToken = default)
    {
        SampleEntity? sample = await context.Samples.FindAsync([query.Id], cancellationToken);

        if (sample is null)
            return Result.Failure<SampleDto>(new Error("Sample.NotFound", "The sample was not found."));

        return Result.Success(new SampleDto(sample.SampleEntityId, sample.Name, sample.Description));
    }
}
