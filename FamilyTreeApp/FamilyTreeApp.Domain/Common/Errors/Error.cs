namespace FamilyTreeApp.Domain.Common.Errors;

public record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", ErrorType.Validation);
    public static readonly Error Validation = new("Error.Validation", "A validation error occurred.", ErrorType.Validation);
    public static readonly Error Failure = new("Error.Failure", "An unexpected error occurred.", ErrorType.Failure);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Conflict = 4,
}
