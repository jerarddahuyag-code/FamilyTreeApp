using FamilyTreeApp.Api.Authorization;
using FamilyTreeApp.Api.Middleware;
using FamilyTreeApp.Application;
using FamilyTreeApp.Infrastructure;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register layered services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<IAuthorizationHandler, TreeAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TreeOwner", p => p.AddRequirements(new TreeOwnerRequirement()));
    options.AddPolicy("TreeAdmin", p => p.AddRequirements(new TreeAdminRequirement()));
    options.AddPolicy("TreeMember", p => p.AddRequirements(new TreeMemberRequirement()));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }); ;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    })
    .AddGoogleOpenIdConnect(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;

        options.SaveTokens = true;

        options.ClaimActions.MapJsonKey("picture", "picture");
        options.ClaimActions.MapJsonKey("given_name", "given_name");
        options.ClaimActions.MapJsonKey("family_name", "family_name");

        options.CallbackPath = "/signin-google";
    });

builder.Services.AddEndpointsApiExplorer();
// Swagger/OpenAPI (Swashbuckle)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FamilyTreeApp API", Version = "v1" });

    c.AddSecurityDefinition("OIDC", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri("https://accounts.google.com/.well-known/openid-configuration")
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("OIDC", document),
            new List<string> { "openid", "profile", "email" }
        }
    });
});
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable Swagger middleware and UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyTreeApp API V1");
    c.RoutePrefix = "swagger"; // serve at /swagger
    // Configure Swagger UI to use Google OIDC client from configuration
    c.OAuthClientId(builder.Configuration["Authentication:Google:ClientId"]);
    c.OAuthClientSecret(builder.Configuration["Authentication:Google:ClientSecret"]);
    c.OAuthAppName("FamilyTreeApp API - Swagger");
    // Use PKCE for the authorization code flow (recommended)
    c.OAuthUsePkce();
});

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
