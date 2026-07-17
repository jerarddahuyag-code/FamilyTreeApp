using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Domain.Events;

public class SampleDomainEvent(Guid sampleId) : IDomainEvent
{
    public Guid SampleId { get; } = sampleId;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
