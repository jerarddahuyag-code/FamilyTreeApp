namespace FamilyTreeApp.Domain.Common.Errors;

public static class DomainErrors
{
    public static class UserErrors
    {
        public readonly static Error UserNotFound = new("User.NotFound", "The user was not found.", ErrorType.NotFound);
        public readonly static Error InvalidEmail = new("User.InvalidEmail", "The provided email address is invalid.", ErrorType.Validation);
        public readonly static Error InvalidProfile = new("User.InvalidProfile", "The provided profile information is invalid.", ErrorType.Validation);
        public readonly static Error UserDeleted = new("User.Deleted", "The user has been deleted.", ErrorType.Conflict);
    }

    public static class TreeErrors
    {
        public readonly static Error TreeNotFound = new("Tree.NotFound", "The tree was not found.", ErrorType.NotFound);
        public readonly static Error InvalidTreeName = new("Tree.InvalidName", "The provided tree name is invalid.", ErrorType.Validation);
        public readonly static Error InvalidTreeRole = new("Tree.InvalidRole", "The provided tree role is invalid.", ErrorType.Validation);
        public readonly static Error TreeDeleted = new("Tree.Delete", "The Tree has been deleted.", ErrorType.Conflict);
    }

    public static class ExternalLoginErrors
    {
        public readonly static Error InvalidProvider = new("ExternalLogin.InvalidProvider", "The provided external login provider is invalid.", ErrorType.Validation);
        public readonly static Error InvalidProviderKey = new("ExternalLogin.InvalidProviderKey", "The provided external login provider key is invalid.", ErrorType.Validation);
    }
}
