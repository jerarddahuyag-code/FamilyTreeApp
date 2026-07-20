using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Domain.Common.Errors;

public static class UserErrors
{
    public readonly static Error UserNotFound = new ("User.NotFound", "The user was not found.");
    public readonly static Error InvalidEmail = new ("User.InvalidEmail", "The provided email address is invalid.");
    public readonly static Error InvalidProfile = new ("User.InvalidProfile", "The provided profile information is invalid.");
    public readonly static Error UserDeleted = new ("User.Deleted", "The user has been deleted.");
}
