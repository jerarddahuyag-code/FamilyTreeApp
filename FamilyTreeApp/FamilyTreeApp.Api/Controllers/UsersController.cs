using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Application.Users.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersQuery request, [FromServices] IQueryHandler<GetUsersQuery, GetUsersQueryResponse> getUsersHandler, CancellationToken cancellationToken)
    {
        Result<GetUsersQueryResponse> result = await getUsersHandler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, [FromServices] IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse> getUserByIdHandler, CancellationToken cancellationToken)
    {
        Result<GetUserByIdQueryResponse> result = await getUserByIdHandler.HandleAsync(new GetUserByIdQuery { UserId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand request, [FromServices] ICommandHandler<CreateUserCommand, Guid> createUserHandler, CancellationToken cancellationToken)
    {
        Result<Guid> result = await createUserHandler.HandleAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/users/{result.Value}", result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, [FromServices] ICommandHandler<DeleteUserCommand, bool> deleteUserHandler, CancellationToken cancellationToken)
    {
        Result<bool> result = await deleteUserHandler.HandleAsync(new DeleteUserCommand { UserId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
