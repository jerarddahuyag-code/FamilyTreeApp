using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Roster.Entities;
using FamilyTreeApp.Domain.Roster.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Application.Roster.CQRS.Queries;

public record GetRelationshipsQuery : IRequest<GetRelationshipQueryResponse>
{
    public required Guid TreeId { get; init; }
    public Guid? MemberId { get; init; }
}

public record GetRelationshipQueryResponse
{
    public required List<GetRelationshipQueryResponseItem> Relationships { get; init; }
}

public record GetRelationshipQueryResponseItem
{
    public required Guid FamilyMemberRelationshipId { get; init; }
    public required Guid TreeId { get; init; }
    public required Guid BaseFamilyMemberId { get; init; }
    public required Guid RelatedFamilyMemberId { get; init; }
    public required RelationshipType RelationshipType { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public class GetRelationshipsQueryHandler(
    IApplicationDbContext dbContext)
    : IQueryHandler<GetRelationshipsQuery, GetRelationshipQueryResponse>
{
    public async Task<Result<GetRelationshipQueryResponse>> HandleAsync(GetRelationshipsQuery query, CancellationToken cancellationToken = default)
    {
        FamilyMemberRelationship[] relationships = [];
        if (query.MemberId.HasValue)
        {
            relationships = await dbContext.FamilyMemberRelationships.Where(r => (r.BaseFamilyMemberId == query.MemberId.Value || r.RelatedFamilyMemberId == query.MemberId.Value) && r.TreeId == query.TreeId).ToArrayAsync(cancellationToken);
        }

        var dtos = relationships.Select(r => new GetRelationshipQueryResponseItem
        {
            FamilyMemberRelationshipId = r.FamilyMemberRelationshipId,
            TreeId = r.TreeId,
            BaseFamilyMemberId = r.BaseFamilyMemberId,
            RelatedFamilyMemberId = r.RelatedFamilyMemberId,
            RelationshipType = r.RelationshipType,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        return Result.Success(new GetRelationshipQueryResponse { Relationships = dtos });
    }
}
