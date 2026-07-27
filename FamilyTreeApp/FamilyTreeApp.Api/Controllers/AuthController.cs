using System.Security.Claims;
using FamilyTreeApp.Application.Users.CQRS.Commands;
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
    ICommandHandler<ProcessExternalLoginCommand, Guid> processExternalLoginHandler) : ApiControllerBase
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

        var processCmd = new ProcessExternalLoginCommand
        {
            Provider = provider,
            ProviderKey = providerKey,
            Email = email,
            Name = name,
            GivenName = givenName,
            FamilyName = familyName,
            Picture = picture
        };

        var processResult = await processExternalLoginHandler.HandleAsync(processCmd, cancellationToken);
        if (processResult.IsFailure)
        {
            return HandleFailure(processResult);
        }

        Guid userId = processResult.Value;

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
