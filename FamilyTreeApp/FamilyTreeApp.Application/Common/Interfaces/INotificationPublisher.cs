namespace FamilyTreeApp.Application.Common.Interfaces;

public interface INotificationPublisher
{
    Task PublishAsync<T>(T notification, CancellationToken cancellationToken = default)
        where T : notnull;
}
