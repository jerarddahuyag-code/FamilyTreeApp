using FamilyTreeApp.Domain.Common;
using FamilyTreeApp.Domain.Trees.Entities;
using FamilyTreeApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var trees = await db.Trees
            .AsNoTracking()
            .Where(t => t.DeletedAt == null)
            .Select(t => new
            {
                t.TreeId,
                t.Name,
                t.Description,
                t.IsPublic,
                t.CreatedAt,
                t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(trees);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var tree = await db.Trees
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TreeId == id && t.DeletedAt == null, cancellationToken);

        if (tree == null)
        {
            return NotFound(new { Message = "Tree not found" });
        }

        return Ok(new
        {
            tree.TreeId,
            tree.Name,
            tree.Description,
            tree.IsPublic,
            tree.CreatedAt,
            tree.UpdatedAt
        });
    }

    public record CreateTreeRequest(string Name, string Description, bool IsPublic);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTreeRequest request, [FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var createResult = Tree.Create(Guid.NewGuid(), request.Name, request.Description, request.IsPublic);
        if (createResult.IsFailure)
        {
            return HandleFailure(createResult);
        }

        var tree = createResult.Value;

        await db.Trees.AddAsync(tree, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/trees/{tree.TreeId}", new { tree.TreeId });
    }

    public record UpdateTreeRequest(Guid TreeId, string Name, string? Description, bool? IsPublic);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTreeRequest request, [FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (id != request.TreeId)
        {
            return BadRequest("Mismatched tree id");
        }

        var tree = await db.Trees.FirstOrDefaultAsync(t => t.TreeId == id && t.DeletedAt == null, cancellationToken);
        if (tree == null)
        {
            return NotFound(new { Message = "Tree not found" });
        }

        var updateResult = tree.UpdateDetails(request.Name, request.Description);
        if (updateResult.IsFailure)
        {
            return HandleFailure(updateResult);
        }

        if (request.IsPublic.HasValue)
        {
            if (request.IsPublic.Value)
            {
                var r = tree.MakePublic();
                if (r.IsFailure) return HandleFailure(r);
            }
            else
            {
                var r = tree.MakePrivate();
                if (r.IsFailure) return HandleFailure(r);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var tree = await db.Trees.FirstOrDefaultAsync(t => t.TreeId == id && t.DeletedAt == null, cancellationToken);
        if (tree == null)
        {
            return NotFound(new { Message = "Tree not found" });
        }

        var result = tree.SoftDelete();
        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
