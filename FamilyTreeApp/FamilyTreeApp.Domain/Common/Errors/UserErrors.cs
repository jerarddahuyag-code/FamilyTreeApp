using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Domain.Common.Errors;

public static class UserErrors
{
    public readonly static Error UserNotFound = new ("User.NotFound", "The user was not found.");
}
