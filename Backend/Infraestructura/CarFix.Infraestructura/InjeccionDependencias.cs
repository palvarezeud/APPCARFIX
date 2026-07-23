using System.Text;
using Anthropic;
using CarFix.Dominio.Interfaces;
using CarFix.Infraestructura.Correo;
using CarFix.Infraestructura.IA;
using CarFix.Infraestructura.Pdf;
using CarFix.Infraestructura.Persistencia;
using CarFix.Infraestructura.Persistencia.Repositorios;
using CarFix.Infraestructura.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CarFix.Infraestructura;

public static class InjeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CarFixDbContext>(opciones =>
            opciones.UseSqlServer(config.GetConnectionString("CarFix"),
                sqlOpciones => sqlOpciones.UseNetTopologySuite()));

        services.AddScoped<ICarFixDbContext>(sp => sp.GetRequiredService<CarFixDbContext>());
        services.AddScoped<IUnidadTrabajo,            UnidadTrabajo>();
        services.AddScoped<IRepositorioCliente,       RepositorioCliente>();
        services.AddScoped<IRepositorioVehiculo,      RepositorioVehiculo>();
        services.AddScoped<IRepositorioOrdenServicio, RepositorioOrdenServicio>();
        services.AddScoped<IRepositorioFactura,       RepositorioFactura>();
        services.AddScoped<IRepositorioUsuario,       RepositorioUsuario>();
        services.AddScoped<IRepositorioTokenRefresco, RepositorioTokenRefresco>();

        services.AddScoped<IServicioToken,       ServicioToken>();
        services.AddScoped<IServicioContrasenna, ServicioContrasenna>();
        services.AddScoped<IServicioHashToken,   ServicioHashToken>();

        services.AddSingleton(sp =>
        {
            var apiKey = config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "Falta configurar Anthropic:ApiKey (dotnet user-secrets en desarrollo, " +
                    "variable de entorno Anthropic__ApiKey en produccion).");
            return new AnthropicClient { ApiKey = apiKey };
        });
        services.AddScoped<IServicioVisionVehiculo, ServicioVisionAnthropic>();
        services.AddScoped<IServicioVisionFacturaRepuesto, ServicioVisionFacturaRepuestoAnthropic>();
        services.AddScoped<IServicioLlamadaAnthropicJson, ServicioLlamadaAnthropicJson>();
        services.AddScoped<IServicioAsistenteVoz, ServicioAsistenteVozAnthropic>();

        services.AddScoped<IServicioGeneradorFacturaPdf, ServicioGeneradorFacturaPdfQuestPdf>();
        services.AddScoped<IServicioEnvioCorreo,         ServicioEnvioCorreoSmtp>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opciones =>
            {
                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = config["Jwt:Emisor"],
                    ValidAudience            = config["Jwt:Audiencia"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(config["Jwt:Llave"]!))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
