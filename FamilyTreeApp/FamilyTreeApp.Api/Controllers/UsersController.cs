using FamilyTreeApp.Application.CQRS.Commands;
using FamilyTreeApp.Application.CQRS.Queries;
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
    public async Task<IActionResult> GetAll(GetUsersQueryHandler query, CancellationToken cancellationToken)
    {
        var users = await query.HandleAsync(new GetUsersQuery { IncludePrivate = false }, cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, GetUserByIdHandler getUserByIdHandler, CancellationToken cancellationToken)
    {
        var user = await getUserByIdHandler.HandleAsync(new GetUserById { UserId = id }, cancellationToken);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user, CreateUserCommandHandler createUserCommandHandler, CancellationToken cancellationToken)
    {
        var createdUserId = await createUserCommandHandler.HandleAsync(new CreateUserCommand
        {
            Email = user.Email,
            FirstName = user.ProfileInfo.FirstName,
            LastName = user.ProfileInfo.LastName,
            BirthDate = user.ProfileInfo.BirthDate,
            AvatarUrl = user.ProfileInfo.AvatarUrl,
            PhoneNumber = user.ProfileInfo.PhoneNumber,
            Gender = user.ProfileInfo.Gender,
            Bio = user.ProfileInfo.Bio
        }, cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = createdUserId }, createdUserId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] User user, CancellationToken cancellationToken)
    {
        if (id != user.UserId)
            return BadRequest("Mismatched user id");

        var existing = await db.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
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
        var existing = await db.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (existing is null)
            return NotFound();

        db.Users.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
