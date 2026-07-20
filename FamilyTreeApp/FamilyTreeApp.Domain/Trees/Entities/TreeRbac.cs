using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Enums;
using FamilyTreeApp.Domain.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Domain.Trees.Entities;

public class TreeRbac : AggregateRoot
{
    public Guid TreeRbacId { get; private set; }

    public Guid TreeId { get; private set; }

    public Guid UserId { get; private set; }

    public TreeRole TreeRole { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Tree Tree { get; private set; } = null!;

    public User User { get; private set; } = null!;

    private TreeRbac() { }

    private TreeRbac(Guid treeRbacId, Guid treeId, Guid userId, TreeRole treeRole)
    {
        TreeRbacId = treeRbacId;
        TreeId = treeId;
        UserId = userId;
        TreeRole = treeRole;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public static Result<TreeRbac> Create(Guid treeRbacId, Guid treeId, Guid userId, TreeRole treeRole)
    {
        var treeRbac = new TreeRbac(treeRbacId, treeId, userId, treeRole);
        return Result.Success(treeRbac);
    }
}
