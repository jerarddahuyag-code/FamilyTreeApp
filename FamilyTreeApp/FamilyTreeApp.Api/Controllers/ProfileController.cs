using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateProfileCommand request, [FromServices] ICommandHandler<UpdateProfileCommand, Guid> updateProfileHandler, CancellationToken cancellationToken)
    {
        // Passing Id should be temporary until we implement authentication and authorization
        request = request with { UserId = id };

        Result<Guid> result = await updateProfileHandler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
