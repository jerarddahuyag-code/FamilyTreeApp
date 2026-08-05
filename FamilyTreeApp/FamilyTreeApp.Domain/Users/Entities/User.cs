using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;
using FamilyTreeApp.Domain.Common.Errors.ValueObjects;
using System.Net.Mail;

namespace FamilyTreeApp.Domain.Users.Entities;

public class User : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public bool IsPublic { get; private set; }
    public ProfileInfo ProfileInfo { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = [];

    private User() { }

    private User(Guid userId, string email, ProfileInfo profile)
    {
        UserId = userId;
        Email = email;
        ProfileInfo = profile;
        IsPublic = false;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(Guid userId, string email, ProfileInfo profile)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(DomainErrors.UserErrors.InvalidEmail);
        }

        if (!IsValidEmail(email))
        {
            return Result.Failure<User>(DomainErrors.UserErrors.InvalidEmail);
        }

        if (profile is null)
        {
            return Result.Failure<User>(DomainErrors.UserErrors.InvalidProfile);
        }

        var user = new User(userId, email.Trim(), profile);
        return Result.Success(user);
    }

    public Result ChangeEmail(string newEmail)
    {
        if (DeletedAt != null)
        {
            return Result.Failure(DomainErrors.UserErrors.UserDeleted);
        }

        if (string.IsNullOrWhiteSpace(newEmail))
        {
            return Result.Failure(DomainErrors.UserErrors.InvalidEmail);
        }

        if (!IsValidEmail(newEmail))
        {
            return Result.Failure(DomainErrors.UserErrors.InvalidEmail);
        }

        if (!Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
        {
            // var old = Email;
            Email = newEmail.Trim();
            UpdatedAt = DateTime.UtcNow;
            // RaiseDomainEvent(new UserEmailChanged(UserId, old, Email));
        }

        return Result.Success();
    }

    public Result UpdateProfile(ProfileInfo newProfile)
    {
        if (DeletedAt != null)
        {
            return Result.Failure(DomainErrors.UserErrors.UserDeleted);
        }

        if (newProfile is null)
        {
            return Result.Failure(DomainErrors.UserErrors.InvalidProfile);
        }

        if (newProfile.BirthDate.HasValue && newProfile.BirthDate.Value.Date > DateTime.UtcNow.Date)
        {
            return Result.Failure(DomainErrors.UserErrors.InvalidProfile);
        }

        ProfileInfo = newProfile;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MakePublic()
    {
        if (DeletedAt != null)
        {
            return Result.Failure(DomainErrors.UserErrors.UserDeleted);
        }

        if (!IsPublic)
        {
            IsPublic = true;
            UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success();
    }

    public Result MakePrivate()
    {
        if (DeletedAt != null)
        {
            return Result.Failure(DomainErrors.UserErrors.UserDeleted);
        }

        if (IsPublic)
        {
            IsPublic = false;
            UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success();
    }

    public Result SoftDelete()
    {
        if (DeletedAt != null)
        {
            return Result.Success();
        }

        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        // RaiseDomainEvent(new UserDeleted(UserId, DeletedAt));
        return Result.Success();
    }
    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
