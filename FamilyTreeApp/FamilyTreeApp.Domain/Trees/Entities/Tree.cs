using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Common.Errors;

namespace FamilyTreeApp.Domain.Trees.Entities;

public class Tree : AggregateRoot
{
    public Guid TreeId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public bool IsPublic { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    private Tree() { }

    private Tree(Guid treeId, string name, string description, bool isPublic)
    {
        TreeId = treeId;
        Name = name;
        Description = description;
        IsPublic = isPublic;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Tree> Create(Guid treeId, string name, string description, bool isPublic)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Tree>(DomainErrors.TreeErrors.InvalidTreeName);
        }

        var tree = new Tree(treeId, name.Trim(), description.Trim(), isPublic);
        return Result.Success(tree);
    }

    public Result UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(DomainErrors.TreeErrors.InvalidTreeName);
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MakePublic()
    {
        if (DeletedAt != null)
        {
            return Result.Failure(DomainErrors.TreeErrors.TreeDeleted);
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
            return Result.Failure(DomainErrors.TreeErrors.TreeDeleted);
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
}
