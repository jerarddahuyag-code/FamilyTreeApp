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
        public readonly static Error TreeAccessNotFound = new("Tree.AccessNotFound", "The specified tree access was not found.", ErrorType.NotFound);
    }

    public static class ExternalLoginErrors
    {
        public readonly static Error InvalidProvider = new("ExternalLogin.InvalidProvider", "The provided external login provider is invalid.", ErrorType.Validation);
        public readonly static Error InvalidProviderKey = new("ExternalLogin.InvalidProviderKey", "The provided external login provider key is invalid.", ErrorType.Validation);
    }

    public static class FamilyMemberErrors
    {
        public readonly static Error FamilyMemberNotFound = new("FamilyMember.NotFound", "The family member was not found.", ErrorType.NotFound);
        public readonly static Error InvalidProfile = new("FamilyMember.InvalidProfile", "The provided profile information is invalid.", ErrorType.Validation);
        public readonly static Error InvalidVisibilityStatus = new("FamilyMember.InvalidVisibilityStatus", "The provided visibility status is invalid.", ErrorType.Validation);
        public readonly static Error FamilyMemberDeleted = new("FamilyMember.Deleted", "The family member has been deleted.", ErrorType.Conflict);
        public readonly static Error InvalidVisibilityTransition = new("FamilyMember.InvalidVisibilityTransition", "The requested visibility transition is invalid.", ErrorType.Validation);
    }

    public static class FamilyMemberRelationshipErrors
    {
        public readonly static Error SameFamilyMembers = new("FamilyMemberRelationship.SameFamilyMembers", "The provided family members are the same.", ErrorType.Validation);
        public readonly static Error MemberTreeMismatch = new("FamilyMemberRelationship.MemberTreeMismatch", "Both family members must belong to the same tree.", ErrorType.Validation);
        public readonly static Error RelationshipNotFound = new("FamilyMemberRelationship.NotFound", "The relationship was not found.", ErrorType.NotFound);
    }
}
