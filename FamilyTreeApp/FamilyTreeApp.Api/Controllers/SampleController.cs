using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleController(
    ICommandHandler<CreateSampleCommand, Guid> createHandler,
    IQueryHandler<GetSampleQuery, SampleDto> getHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSampleCommand command, CancellationToken cancellationToken)
    {
        Result<Guid> result = await createHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        Result<SampleDto> result = await getHandler.HandleAsync(new GetSampleQuery(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }
}
