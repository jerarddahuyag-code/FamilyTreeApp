using FamilyTreeApp.Application.Trees.CQRS.Commands;
using FamilyTreeApp.Application.Trees.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TreesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTrees([FromQuery] GetTreesQuery request, [FromServices] IQueryHandler<GetTreesQuery, GetTreesQueryResponse> handler, CancellationToken cancellationToken)
    {
        Result<GetTreesQueryResponse> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTreeById(Guid id, [FromServices] IQueryHandler<GetTreeByIdQuery, GetTreeByIdQueryResponse> handler, CancellationToken cancellationToken)
    {
        Result<GetTreeByIdQueryResponse> result = await handler.HandleAsync(new GetTreeByIdQuery { TreeId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTree(CreateTreeCommand request, [FromServices] ICommandHandler<CreateTreeCommand, Guid> handler, CancellationToken cancellationToken)
    {
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/trees/{result.Value}", new { TreeId = result.Value });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateTree(Guid id, [FromBody] UpdateTreeCommand request, [FromServices] ICommandHandler<UpdateTreeCommand, bool> handler, CancellationToken cancellationToken)
    {
        request = request with { TreeId = id };
        Result<bool> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTree(Guid id, [FromServices] ICommandHandler<DeleteTreeCommand, bool> handler, CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(new DeleteTreeCommand { TreeId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPost("{treeId:guid}/access/{userId:guid}")]
    public async Task<IActionResult> AddTreeAccess(Guid treeId, Guid userId, [FromBody] AddTreeAccessCommand request, [FromServices] ICommandHandler<AddTreeAccessCommand, Guid> handler, CancellationToken cancellationToken)
    {
        request = request with { TreeId = treeId, UserId = userId };
        Result<Guid> result = await handler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("{treeId:guid}/access/{userId:guid}")]
    public async Task<IActionResult> RemoveTreeAccess(Guid treeId, Guid userId, [FromServices] ICommandHandler<RemoveTreeAccessCommand, bool> handler, CancellationToken cancellationToken)
    {
        Result<bool> result = await handler.HandleAsync(new RemoveTreeAccessCommand { TreeId = treeId, UserId = userId }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
