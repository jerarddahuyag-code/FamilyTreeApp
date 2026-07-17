namespace FamilyTreeApp.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
