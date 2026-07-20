using FamilyTreeApp.Application.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController() : ControllerBase
{
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileCommand command, [FromServices] ICommandHandler<UpdateProfileCommand, Guid> updateProfileHandler, CancellationToken cancellationToken)
    {
        if (id != command.UserId)
            return BadRequest("Mismatched user id");

        Result<Guid> result = await updateProfileHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Error.NotFound")
                return NotFound();

            return BadRequest(result.Error.Message);
        }

        return NoContent();
    }
}
