using FamilyTreeApp.Application.Users.CQRS.Commands;
using FamilyTreeApp.Domain.Common;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyTreeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IConfiguration config,
    ICommandHandler<ProcessExternalLoginCommand, bool> processExternalLoginHandler) : ApiControllerBase
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
        AuthenticateResult authResult = await HttpContext.AuthenticateAsync(GoogleOpenIdConnectDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
        {
            return BadRequest("External authentication failed.");
        }

        ClaimsPrincipal externalPrincipal = authResult.Principal;

        Result<bool> processResult = await processExternalLoginHandler.HandleAsync(new ProcessExternalLoginCommand
        {
            Provider = "Google",
            ProviderKey = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Email = externalPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            Name = externalPrincipal.FindFirst(ClaimTypes.Name)?.Value,
            GivenName = externalPrincipal.FindFirst("given_name")?.Value,
            FamilyName = externalPrincipal.FindFirst("family_name")?.Value,
            Picture = externalPrincipal.FindFirst("picture")?.Value
        }, cancellationToken);

        if (processResult.IsFailure)
        {
            return HandleFailure(processResult);
        }

        var frontendRedirectUri = authResult.Properties?.Items.TryGetValue("frontend_redirect", out var uri) is true
            ? uri!
            : config["Frontend:RedirectUri"] ?? "/";

        return Redirect(frontendRedirectUri);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var frontendRedirect = returnUrl ?? config["Frontend:RedirectUri"] ?? "/";
        return Redirect(frontendRedirect);
    }
}
