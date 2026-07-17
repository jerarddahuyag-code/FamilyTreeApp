using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Events;

namespace FamilyTreeApp.Domain.Entities;

public class SampleEntity : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    private SampleEntity(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public static SampleEntity Create(string name, string description)
    {
        SampleEntity sample = new SampleEntity(name, description);
        sample.RaiseDomainEvent(new SampleDomainEvent(sample.Id));
        return sample;
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
    }
}
