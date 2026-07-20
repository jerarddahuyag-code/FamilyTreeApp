using FamilyTreeApp.Application.Common.Behaviors;
using FamilyTreeApp.Domain.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FamilyTreeApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register all command handlers via Scrutor assembly scanning
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register all query handlers via Scrutor assembly scanning
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Decorate command handlers with validation pipeline behavior
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationPipelineBehavior<,>));

        return services;
    }
}
