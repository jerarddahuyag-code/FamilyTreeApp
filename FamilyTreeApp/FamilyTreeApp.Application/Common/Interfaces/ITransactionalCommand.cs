namespace FamilyTreeApp.Application.Common.Interfaces;

/// <summary>
/// Marker interface. Commands implementing this are automatically wrapped
/// in a database transaction by <see cref="TransactionBehavior{TRequest,TResponse}"/>.
/// </summary>
public interface ITransactionalCommand
{
}
