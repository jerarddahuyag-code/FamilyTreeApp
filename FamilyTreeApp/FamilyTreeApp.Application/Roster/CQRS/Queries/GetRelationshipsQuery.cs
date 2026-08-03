using FamilyTreeApp.Application.Roster.DTOs;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Interfaces;

namespace FamilyTreeApp.Application.Roster.CQRS.Queries;

public record GetRelationshipsQuery : IRequest<List<RelationshipDto>>
{
    public required Guid TreeId { get; init; }
    public Guid? MemberId { get; init; }
}

public class GetRelationshipsQueryHandler(
    IFamilyMemberRelationshipRepository relationshipRepository)
    : IQueryHandler<GetRelationshipsQuery, List<RelationshipDto>>
{
    public async Task<Result<List<RelationshipDto>>> HandleAsync(GetRelationshipsQuery query, CancellationToken cancellationToken = default)
    {
        List<FamilyMemberRelationship> relationships;
        if (query.MemberId.HasValue)
        {
            relationships = await relationshipRepository.GetByMemberIdAsync(query.MemberId.Value, cancellationToken);
            relationships = relationships.Where(r => r.TreeId == query.TreeId).ToList();
        }
        else
        {
            relationships = await relationshipRepository.GetByTreeIdAsync(query.TreeId, cancellationToken);
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
