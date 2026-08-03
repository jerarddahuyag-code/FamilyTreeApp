using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Application.Roster.CQRS.Queries;
using FamilyTreeApp.Application.Roster.DTOs;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/trees/{treeId:guid}")]
public class RosterController : ApiControllerBase
{
    [HttpGet("members")]
    [Authorize(Policy = "TreeMember")]
    public async Task<IActionResult> GetMembers(
        Guid treeId,
        [FromServices] IQueryHandler<GetFamilyMembersQuery, List<FamilyMemberDto>> handler,
        CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result<List<FamilyMemberDto>> result = await handler.HandleAsync(
            new GetFamilyMembersQuery { TreeId = treeId, UserId = userId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("members")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> AddMember(
        Guid treeId,
        [FromBody] AddFamilyMemberCommand request,
        [FromServices] ICommandHandler<AddFamilyMemberCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId };
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/trees/{treeId}/members/{result.Value}", new { FamilyMemberId = result.Value });
    }

    [HttpPut("members/{memberId:guid}")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> UpdateMember(
        Guid treeId,
        Guid memberId,
        [FromBody] UpdateFamilyMemberCommand request,
        [FromServices] ICommandHandler<UpdateFamilyMemberCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId, FamilyMemberId = memberId };
        Result<bool> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("members/{memberId:guid}")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> DeleteMember(
        Guid treeId,
        Guid memberId,
        [FromServices] ICommandHandler<DeleteFamilyMemberCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(
            new DeleteFamilyMemberCommand { TreeId = treeId, FamilyMemberId = memberId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPost("members/{memberId:guid}/visibility")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> UpdateVisibility(
        Guid treeId,
        Guid memberId,
        [FromBody] RequestVisibilityCommand request,
        [FromServices] ICommandHandler<RequestVisibilityCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId, FamilyMemberId = memberId };
        Result<bool> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpGet("members/{memberId:guid}/relationships")]
    [Authorize(Policy = "TreeMember")]
    public async Task<IActionResult> GetRelationships(
        Guid treeId,
        Guid memberId,
        [FromServices] IQueryHandler<GetRelationshipsQuery, List<RelationshipDto>> handler,
        CancellationToken cancellationToken)
    {
        Result<List<RelationshipDto>> result = await handler.HandleAsync(
            new GetRelationshipsQuery { TreeId = treeId, MemberId = memberId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("members/{memberId:guid}/relationships")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> AddRelationship(
        Guid treeId,
        Guid memberId,
        [FromBody] AddRelationshipCommand request,
        [FromServices] ICommandHandler<AddRelationshipCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId, BaseFamilyMemberId = memberId };
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/trees/{treeId}/members/{memberId}/relationships/{result.Value}", new { FamilyMemberRelationshipId = result.Value });
    }

    [HttpDelete("members/{memberId:guid}/relationships/{relationshipId:guid}")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> RemoveRelationship(
        Guid treeId,
        Guid memberId,
        Guid relationshipId,
        [FromServices] ICommandHandler<RemoveRelationshipCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(
            new RemoveRelationshipCommand { TreeId = treeId, FamilyMemberRelationshipId = relationshipId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
