using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Application.Common.Interfaces;

public interface IAuthService
{
    Task SignInAsync(Guid userId, string email, string? name);
}
