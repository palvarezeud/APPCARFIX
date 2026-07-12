using System.Reflection;
using CarFix.Aplicacion.Comun.Comportamientos;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarFix.Aplicacion;

public static class InjeccionDependencias
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamientoValidacion<,>));
        return services;
    }
}
