using FamilyTreeApp.Application;
using FamilyTreeApp.Infrastructure;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register layered services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }); ;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
