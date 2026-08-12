using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyTreeApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ApiControllerBase
{
    [HttpPatch]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileCommand request, [FromServices] ICommandHandler<UpdateProfileCommand, bool> updateProfileHandler, CancellationToken cancellationToken)
    {
        request = request with { User = User };

        Result<bool> result = await updateProfileHandler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
