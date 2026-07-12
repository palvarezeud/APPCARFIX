using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarFix.Infraestructura.Persistencia;

public static class InicializadorDatos
{
    public static async Task InicializarAsync(IServiceProvider servicios)
    {
        using var alcance = servicios.CreateScope();
        var contexto    = alcance.ServiceProvider.GetRequiredService<CarFixDbContext>();
        var contrasenna = alcance.ServiceProvider.GetRequiredService<IServicioContrasenna>();
        var config      = alcance.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger      = alcance.ServiceProvider.GetRequiredService<ILogger<CarFixDbContext>>();

        if (await contexto.Usuarios.AnyAsync())
            return;

        var usuarios = new[]
        {
            new Usuario
            {
                NombreUsuario  = config["Seed:AdminNombreUsuario"]  ?? "admin",
                PasswordHash   = contrasenna.Hashear(config["Seed:AdminPassword"] ?? "Admin2024!"),
                NombreCompleto = config["Seed:AdminNombreCompleto"] ?? "Administrador del Sistema",
                Email          = null,
                RolId          = 1, // Administrador
                Activo         = true
            },
            new Usuario
            {
                NombreUsuario  = config["Seed:JefeNombreUsuario"]  ?? "jefe",
                PasswordHash   = contrasenna.Hashear(config["Seed:JefePassword"] ?? "Jefe2024!"),
                NombreCompleto = config["Seed:JefeNombreCompleto"] ?? "Jefe de Mecanicos",
                Email          = null,
                RolId          = 2, // JefeMecanicos
                Activo         = true
            }
        };

        contexto.Usuarios.AddRange(usuarios);
        await contexto.SaveChangesAsync();

        logger.LogInformation("Usuarios iniciales creados: {Usuarios}",
            string.Join(", ", usuarios.Select(u => u.NombreUsuario)));
    }
}
