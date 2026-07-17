using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyTreeApp.Application.CQRS.Queries;

public record GetUsersQuery
{
    public required bool IncludePrivate { get; init; }
}

public class GetUsersQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetUsersQuery, List<User>>
{
    public async Task<Result<List<User>>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await context.Users
            .Where(u => query.IncludePrivate || u.IsPublic)
            .ToListAsync(cancellationToken);
        
        return Result.Success(users);
    }
}
