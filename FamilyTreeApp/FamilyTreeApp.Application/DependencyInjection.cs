using FamilyTreeApp.Application.Common.Behaviors;
using FamilyTreeApp.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FamilyTreeApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Behaviors — registered innermost-first (LIFO = last registered = outermost)
        services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionBehavior<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
