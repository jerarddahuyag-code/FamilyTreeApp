using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Application.Roster.DTOs;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Queries;

public record GetRelationshipsQuery : IRequest<List<RelationshipDto>>
{
    public required Guid TreeId { get; init; }
    public Guid? MemberId { get; init; }
}

public class GetRelationshipsQueryHandler(
    IApplicationDbContext dbContext)
    : IQueryHandler<GetRelationshipsQuery, List<RelationshipDto>>
{
    public async Task<Result<List<RelationshipDto>>> HandleAsync(GetRelationshipsQuery query, CancellationToken cancellationToken = default)
    {
        FamilyMemberRelationship[] relationships = [];
        if (query.MemberId.HasValue)
        {
            relationships = await dbContext.FamilyMemberRelationships.Where(r => (r.BaseFamilyMemberId == query.MemberId.Value || r.RelatedFamilyMemberId == query.MemberId.Value) && r.TreeId == query.TreeId).ToArrayAsync(cancellationToken);
        }

        var dtos = relationships.Select(r => new RelationshipDto
        {
            FamilyMemberRelationshipId = r.FamilyMemberRelationshipId,
            TreeId = r.TreeId,
            BaseFamilyMemberId = r.BaseFamilyMemberId,
            RelatedFamilyMemberId = r.RelatedFamilyMemberId,
            RelationshipType = r.RelationshipType,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        return Result.Success(dtos);
    }
}
