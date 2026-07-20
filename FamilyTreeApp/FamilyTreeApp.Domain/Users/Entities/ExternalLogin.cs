using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Domain.Users.Entities;

public class ExternalLogin : AggregateRoot
{
    public Guid ExternalLoginId { get; private set; }

    public Guid UserId { get; private set; }

    public string Provider { get; private set; } = null!;

    public string ProviderKey { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private ExternalLogin() { }

    private ExternalLogin(Guid externalLoginId, Guid userId, string provider, string providerKey)
    {
        ExternalLoginId = externalLoginId;
        UserId = userId;
        Provider = provider;
        ProviderKey = providerKey;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<ExternalLogin> Create(Guid externalLoginId, Guid userId, string provider, string providerKey)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure<ExternalLogin>(DomainErrors.ExternalLoginErrors.InvalidProvider);
        }

        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return Result.Failure<ExternalLogin>(DomainErrors.ExternalLoginErrors.InvalidProviderKey);
        }

        var externalLogin = new ExternalLogin(externalLoginId, userId, provider.Trim(), providerKey.Trim());
        return Result.Success(externalLogin);
    }
}
