using FamilyTreeApp.Application.CQRS.Commands;
using FamilyTreeApp.Application.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromBody] GetUsersQuery query, [FromServices] IQueryHandler<GetUsersQuery, GetUsersQueryResponse> getUsersHandler, CancellationToken cancellationToken)
    {
        Result<GetUsersQueryResponse> result = await getUsersHandler.HandleAsync(query, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse> getUserByIdHandler, CancellationToken cancellationToken)
    {
        Result<GetUserByIdQueryResponse>? result = await getUserByIdHandler.HandleAsync(new GetUserByIdQuery { UserId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, [FromServices] ICommandHandler<CreateUserCommand, Guid> createUserHandler, CancellationToken cancellationToken)
    {
        Result<Guid> result = await createUserHandler.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return Created($"/api/users/{result.Value}", result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ICommandHandler<DeleteUserCommand, bool> deleteUserHandler, CancellationToken cancellationToken)
    {
        Result<bool> result = await deleteUserHandler.HandleAsync(new DeleteUserCommand { UserId = id }, cancellationToken);
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}
