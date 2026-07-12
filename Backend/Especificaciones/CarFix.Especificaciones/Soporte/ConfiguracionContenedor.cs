using CarFix.Aplicacion;
using CarFix.Dominio.Interfaces;
using CarFix.Infraestructura.Persistencia;
using CarFix.Infraestructura.Persistencia.Repositorios;
using CarFix.Infraestructura.Seguridad;
using CarFix.Especificaciones.Soporte;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace CarFix.Especificaciones;

public class ConfiguracionContenedor
{
    [ScenarioDependencies]
    public static IServiceCollection ConfigurarServicios()
    {
        var servicios = new ServiceCollection();

        servicios.AddDbContext<CarFixDbContext>(op =>
            op.UseInMemoryDatabase("CarFixTest"));

        servicios.AddScoped<ICarFixDbContext>(sp => sp.GetRequiredService<CarFixDbContext>());
        servicios.AddScoped<IUnidadTrabajo,            CarFix.Infraestructura.Persistencia.UnidadTrabajo>();
        servicios.AddScoped<IRepositorioCliente,       RepositorioCliente>();
        servicios.AddScoped<IRepositorioVehiculo,      RepositorioVehiculo>();
        servicios.AddScoped<IRepositorioOrdenServicio, RepositorioOrdenServicio>();
        servicios.AddScoped<IRepositorioFactura,       RepositorioFactura>();

        servicios.AddScoped<IServicioContrasenna, ServicioContrasenna>();
        servicios.AddScoped<IServicioToken,       ServicioTokenFalso>();

        servicios.AddLogging();
        servicios.AddScoped<ContextoEscenario>();
        servicios.AgregarAplicacion();

        return servicios;
    }
}
