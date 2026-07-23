using System.Security.Claims;
using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Application.Common.Interfaces;
using FamilyTreeApp.Domain.Users.Entities;
using FamilyTreeApp.Domain.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IConfiguration config,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICommandHandler<CreateUserCommand, Guid> createUserHandler) : ControllerBase
{

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var frontendRedirect = returnUrl ?? config["Frontend:RedirectUri"] ?? "/";

        var props = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/callback",
        };
        props.Items["frontend_redirect"] = frontendRedirect;

        return Challenge(props);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(CancellationToken cancellationToken)
    {
        var authResult = await HttpContext.AuthenticateAsync(GoogleOpenIdConnectDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            return BadRequest("External authentication failed.");
        }

        var externalPrincipal = authResult.Principal;
        string provider = "Google";
        string? providerKey = externalPrincipal.FindFirst("sub")?.Value
                              ?? externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        string? email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value
                        ?? externalPrincipal.FindFirst("email")?.Value;

        string? name = externalPrincipal.FindFirst(ClaimTypes.Name)?.Value
                       ?? externalPrincipal.FindFirst("name")?.Value;
        string? givenName = externalPrincipal.FindFirst("given_name")?.Value;
        string? familyName = externalPrincipal.FindFirst("family_name")?.Value;
        string? picture = externalPrincipal.FindFirst("picture")?.Value;

        if (string.IsNullOrEmpty(providerKey) || string.IsNullOrEmpty(email))
        {
            return BadRequest("Missing required claims from provider.");
        }

        var existingLogin = await db.ExternalLogins
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, cancellationToken);

        Guid userId;
        if (existingLogin != null)
        {
            userId = existingLogin.UserId;
        }
        else
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
            {
                userId = existingUser.UserId;
            }
            else
            {
                var createUserCmd = new CreateUserCommand
                {
                    Email = email,
                    FirstName = givenName ?? (name?.Split(' ').FirstOrDefault()),
                    LastName = familyName ?? (name?.Split(' ').Skip(1).FirstOrDefault()),
                    AvatarUrl = picture,
                    IsPublic = true
                };

                var createResult = await createUserHandler.HandleAsync(createUserCmd, cancellationToken);
                if (createResult.IsFailure)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, createResult.Error);
                }

                userId = createResult.Value;
            }

            var createExternalResult = ExternalLogin.Create(Guid.NewGuid(), userId, provider, providerKey);
            if (createExternalResult.IsFailure)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, createExternalResult.Error);
            }

            await db.ExternalLogins.AddAsync(createExternalResult.Value, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };
        if (!string.IsNullOrEmpty(name)) {
            claims.Add(new(ClaimTypes.Name, name));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // redirect to frontend URL (we stored it in Items on the Challenge)
        string frontendRedirectUri = authResult.Properties?.Items.TryGetValue("frontend_redirect", out var uri) is true
            ? uri!
            : config["Frontend:RedirectUri"] ?? "/";

        return Redirect(frontendRedirectUri);
    }
}
