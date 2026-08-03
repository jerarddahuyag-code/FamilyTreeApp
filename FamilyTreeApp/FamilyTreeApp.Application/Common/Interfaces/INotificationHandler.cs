namespace FamilyTreeApp.Application.Common.Interfaces;

public interface INotificationHandler<in T> where T : notnull
{
    Task HandleAsync(T notification, CancellationToken cancellationToken = default);
}
