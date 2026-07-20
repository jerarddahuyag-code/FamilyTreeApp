using FamilyTreeApp.Application.CQRS.Commands;
using FamilyTreeApp.Application.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController() : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromBody] GetUsersQuery query, [FromServices] IQueryHandler<GetUsersQuery, GetUsersQueryResponse> getUsersHandler, CancellationToken cancellationToken)
    {
        Result<GetUsersQueryResponse> users = await getUsersHandler.HandleAsync(query, cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse> getUserByIdHandler, CancellationToken cancellationToken)
    {
        Result<GetUserByIdQueryResponse>? user = await getUserByIdHandler.HandleAsync(new GetUserByIdQuery { UserId = id }, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, [FromServices] ICommandHandler<CreateUserCommand, Guid> createUserHandler, CancellationToken cancellationToken)
    {
        Result<Guid> createdUserId = await createUserHandler.HandleAsync(command, cancellationToken);

        return Created($"/api/users/{createdUserId}", createdUserId);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteUserCommand, bool> deleteUserHandler, CancellationToken cancellationToken)
    {
        Result<bool> result = await deleteUserHandler.HandleAsync(new DeleteUserCommand { UserId = id }, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        return NoContent();
    }
}
