using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.DTOs;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using FamilyTreeApp.Domain.Trees.Enums;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Queries;

public record GetFamilyMembersQuery : IRequest<List<FamilyMemberDto>>
{
    public required Guid TreeId { get; init; }
    public required Guid UserId { get; init; }
}

public class GetFamilyMembersQueryHandler(
    IApplicationDbContext dbContext)
    : IQueryHandler<GetFamilyMembersQuery, List<FamilyMemberDto>>
{
    public async Task<Result<List<FamilyMemberDto>>> HandleAsync(GetFamilyMembersQuery query, CancellationToken cancellationToken = default)
    {
        var isAdmin = await dbContext.TreeRbacs
            .AnyAsync(r => r.TreeId == query.TreeId && r.UserId == query.UserId && (r.TreeRole == TreeRole.Admin || r.TreeRole == TreeRole.Owner), cancellationToken);

        List<FamilyMember> members = await dbContext.FamilyMembers.Include(m => m.Relationships).Where(m => m.TreeId == query.TreeId).ToListAsync(cancellationToken);

        // Fetch linked users for Read-Through Profile Merge
        var claimedUserIds = members
            .Where(m => m.ClaimedByUserId.HasValue)
            .Select(m => m.ClaimedByUserId!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, User> userDict = [];
        if (claimedUserIds.Count != 0)
        {
            userDict = await dbContext.Users
                .Where(u => claimedUserIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, cancellationToken);
        }

        List<FamilyMemberDto> dtos = [];

        foreach (FamilyMember member in members)
        {
            ProfileInfo profile = member.ProfileInfo;

            // R-3.4 Read-Through Profile Merge
            if (member.ClaimedByUserId.HasValue && userDict.TryGetValue(member.ClaimedByUserId.Value, out User? user))
            {
                profile = MergeProfile(user.ProfileInfo, member.ProfileInfo);
            }

            // R-3.5 Anonymous Masking
            if (member.VisibilityStatus != VisibilityStatus.Visible && !isAdmin)
            {
                profile = ProfileInfo.CreateAnonymous();
            }

            dtos.Add(new FamilyMemberDto
            {
                FamilyMemberId = member.FamilyMemberId,
                TreeId = member.TreeId,
                ClaimedByUserId = member.ClaimedByUserId,
                ProfileInfo = profile,
                VisibilityStatus = member.VisibilityStatus,
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            });
        }

        return Result.Success(dtos);
    }

    private static ProfileInfo MergeProfile(ProfileInfo userProfile, ProfileInfo memberProfile)
    {
        return new ProfileInfo
        {
            FirstName = !string.IsNullOrWhiteSpace(userProfile.FirstName) ? userProfile.FirstName : memberProfile.FirstName,
            LastName = !string.IsNullOrWhiteSpace(userProfile.LastName) ? userProfile.LastName : memberProfile.LastName,
            Gender = userProfile.Gender.HasValue ? userProfile.Gender : memberProfile.Gender,
            BirthDate = userProfile.BirthDate ?? memberProfile.BirthDate,
            Bio = userProfile.Bio ?? memberProfile.Bio,
            AvatarUrl = userProfile.AvatarUrl ?? memberProfile.AvatarUrl,
            PhoneNumber = userProfile.PhoneNumber ?? memberProfile.PhoneNumber
        };
    }
}
