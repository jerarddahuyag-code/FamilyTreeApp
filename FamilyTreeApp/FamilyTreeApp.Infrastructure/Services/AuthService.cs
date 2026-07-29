using FamilyTreeApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using FamilyTreeApp.Domain.Common;

namespace FamilyTreeApp.Infrastructure.Services;

public class AuthService(IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task SignInAsync(Guid userId, string email, string? name)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };
        if (!string.IsNullOrEmpty(name))
        {
            claims.Add(new(ClaimTypes.Name, name));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
