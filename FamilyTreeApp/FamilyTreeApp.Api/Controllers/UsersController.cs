using FamilyTreeApp.Application.CQRS.Commands;
using FamilyTreeApp.Application.CQRS.Queries;
using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Entities;
using FamilyTreeApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromBody] GetUsersQuery query, [FromServices] IQueryHandler<GetUsersQuery, List<User>> getUsersHandler, CancellationToken cancellationToken)
    {
        Result<List<User>> users = await getUsersHandler.HandleAsync(query, cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse> getUserByIdHandler, CancellationToken cancellationToken)
    {
        Result<GetUserByIdQueryResponse>? user = await getUserByIdHandler.HandleAsync(new GetUserByIdQuery { UserId = id }, cancellationToken);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, [FromServices] ICommandHandler<CreateUserCommand, Guid> createUserHandler, CancellationToken cancellationToken)
    {
        Result<Guid> createdUserId = await createUserHandler.HandleAsync(command, cancellationToken);

        return Created($"/api/users/{createdUserId}", createdUserId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] User user, CancellationToken cancellationToken)
    {
        if (id != user.UserId)
            return BadRequest("Mismatched user id");

        User? existing = await db.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (existing is null)
            return NotFound();

        existing.Email = user.Email;
        existing.IsPublic = user.IsPublic;
        existing.ProfileInfo = user.ProfileInfo;
        existing.UpdatedAt = DateTime.UtcNow;

        db.Users.Update(existing);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        User? existing = await db.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (existing is null)
            return NotFound();

        db.Users.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
