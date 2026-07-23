using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileCommand command, [FromServices] ICommandHandler<UpdateProfileCommand, Guid> updateProfileHandler, CancellationToken cancellationToken)
    {
        // This should be temporary until we implement authentication and authorization
        if (id != command.UserId)
        {
            return BadRequest("Mismatched user id");
        }

        Result<Guid> result = await updateProfileHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
