using CarFix.Aplicacion;
using CarFix.Infraestructura;
using CarFix.Infraestructura.Persistencia;
using CarFix.WebApi.Endpoints;
using CarFix.WebApi.Excepciones;
using CarFix.WebApi.OpenApi;
using Serilog;
using Serilog.Events;
using QuestPDF.Infrastructure;

// Licencia Community de QuestPDF: gratuita para empresas con ingresos anuales menores a $1M USD.
QuestPDF.Settings.License = LicenseType.Community;

// Configurar Serilog antes de crear el builder para capturar errores de arranque
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore",                    LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database",  LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query",     LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/carfix-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AgregarAplicacion()
        .AgregarInfraestructura(builder.Configuration);

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<ManejadorExcepciones>();

    builder.Services.AddCors(opciones =>
        opciones.AddPolicy("OrigenAngular", politica =>
            politica
                .WithOrigins("http://localhost:4200", "http://192.168.10.108:4200", "https://192.168.10.108:4200", "https://icy-wave-040fef60f.7.azurestaticapps.net")
                .AllowAnyHeader()
                .AllowAnyMethod()));

    builder.Services.AddOpenApi(opciones =>
        opciones.AddDocumentTransformer<TransformadorSeguridadJwt>());

    var app = builder.Build();

    await InicializadorDatos.InicializarAsync(app.Services);

    app.UseExceptionHandler();

    // Registra cada request HTTP: metodo, ruta, codigo de respuesta y duracion
    app.UseSerilogRequestLogging(opciones =>
    {
        opciones.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â ÃƒÂ¢Ã¢â€šÂ¬Ã¢â€žÂ¢ {StatusCode} ({Elapsed:0}ms)";
    });

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseCors("OrigenAngular");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapearSalud();
    app.MapearAutenticacion();
    app.MapearClientes();
    app.MapearVehiculos();
    app.MapearOrdenes();
    app.MapearFacturas();
    app.MapearReparaciones();
    app.MapearRepuestos();
    app.MapearTiposReparacion();
    app.MapearHistoricoRepuestos();
    app.MapearMarcasModelos();
    app.MapearUsuarios();
    app.MapearTalleres();
    app.MapearParametros();
    app.MapearAsistenteVoz();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
