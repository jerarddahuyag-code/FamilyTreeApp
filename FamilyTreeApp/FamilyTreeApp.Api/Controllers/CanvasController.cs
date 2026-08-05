using FamilyTreeApp.Application.Canvas.Commands;
using FamilyTreeApp.Application.Canvas.DTOs;
using FamilyTreeApp.Application.Canvas.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyTreeApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/trees/{treeId:guid}/canvas")]
public class CanvasController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = "TreeMember")]
    public async Task<IActionResult> GetCanvas(
        Guid treeId,
        [FromServices] IQueryHandler<GetCanvasQuery, GetCanvasQueryResponse> handler,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        Result<GetCanvasQueryResponse> result = await handler.HandleAsync(
            new GetCanvasQuery { TreeId = treeId, RequestingUserId = userId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPut]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> UpdateCanvas(
        Guid treeId,
        [FromBody] UpdateCanvasCommand request,
        [FromServices] ICommandHandler<UpdateCanvasCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId };
        Result<bool> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPost("nodes")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> AddTreeNode(
        Guid treeId,
        [FromBody] AddTreeNodeCommand request,
        [FromServices] ICommandHandler<AddTreeNodeCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId };
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/v1/trees/{treeId}/canvas/nodes/{result.Value}", new { NodeId = result.Value });
    }

    [HttpDelete("nodes/{nodeId:guid}")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> RemoveTreeNode(
        Guid treeId,
        Guid nodeId,
        [FromServices] ICommandHandler<RemoveTreeNodeCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(
            new RemoveTreeNodeCommand { TreeId = treeId, NodeId = nodeId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPost("edges")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> AddTreeEdge(
        Guid treeId,
        [FromBody] AddTreeEdgeCommand request,
        [FromServices] ICommandHandler<AddTreeEdgeCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId };
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/v1/trees/{treeId}/canvas/edges/{result.Value}", new { EdgeId = result.Value });
    }

    [HttpDelete("edges/{edgeId:guid}")]
    [Authorize(Policy = "TreeAdmin")]
    public async Task<IActionResult> RemoveTreeEdge(
        Guid treeId,
        Guid edgeId,
        [FromServices] ICommandHandler<RemoveTreeEdgeCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(
            new RemoveTreeEdgeCommand { TreeId = treeId, EdgeId = edgeId },
            cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
