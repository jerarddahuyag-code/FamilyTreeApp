using FamilyTreeApp.Application.Roster.CQRS.Commands;
using FamilyTreeApp.Application.Roster.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [FromServices] IQueryHandler<GetFamilyMembersQuery, GetFamilyMembersResponse> handler,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result<GetFamilyMembersResponse> result = await handler.HandleAsync(
            new GetFamilyMembersQuery { TreeId = treeId, UserId = userId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result);
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

    [HttpPut("members/{memberId:guid}/profile")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> UpdateMemberProfile(
        Guid treeId,
        Guid memberId,
        [FromBody] UpdateFamilyMemberProfileCommand request,
        [FromServices] ICommandHandler<UpdateFamilyMemberProfileCommand, bool> handler,
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

    [HttpPut("members/{memberId:guid}/user")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> UpdateMemberClaimedUser(
        Guid treeId,
        Guid memberId,
        [FromBody] UpdateFamilyMemberClaimedUserCommand request,
        [FromServices] ICommandHandler<UpdateFamilyMemberClaimedUserCommand, bool> handler,
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

    [HttpPut("members/{memberId:guid}/claim")]
    [Authorize(Policy = "TreeMember")]
    public async Task<IActionResult> ClaimMember(
        Guid treeId,
        Guid memberId,
        [FromServices] ICommandHandler<ClaimFamilyMemberCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result<bool> result = await handler.HandleAsync(
            new ClaimFamilyMemberCommand { TreeId = treeId, FamilyMemberId = memberId, UserId = userId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("members/{memberId:guid}/unclaim")]
    [Authorize(Policy = "TreeMember")]
    public async Task<IActionResult> UnclaimMember(
        Guid treeId,
        Guid memberId,
        [FromServices] ICommandHandler<UnclaimFamilyMemberCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result<bool> result = await handler.HandleAsync(
            new UnclaimFamilyMemberCommand { TreeId = treeId, FamilyMemberId = memberId, UserId = userId },
            cancellationToken);

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
        [FromServices] IQueryHandler<GetRelationshipsQuery, GetRelationshipQueryResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<GetRelationshipQueryResponse> result = await handler.HandleAsync(
            new GetRelationshipsQuery { TreeId = treeId, MemberId = memberId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result);
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
