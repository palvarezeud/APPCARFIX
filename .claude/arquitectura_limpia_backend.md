# Guia de Arquitectura Limpia — Backend ASP.NET Core (.NET 10 / C#)

Backend del proyecto CAR_FIX usando **ASP.NET Core Web API, .NET 10, Entity Framework Core Database First, JWT**.

---

## 0. Convenciones de Nomenclatura

**Todo el codigo se escribe en espannol, sin caracteres especiales.**

- Sin tildes: `numero` no `número`, `pagina` no `página`
- Sin nn: `annio` en lugar de `año`, `espannol` en lugar de `español`
- Los nombres de entidades y propiedades deben coincidir **exactamente** con los nombres de las tablas y columnas en la base de datos
- Sufijos tecnicos pueden quedar en ingles: `Repository`, `Handler`, `Validator`, `Command`, `Query`, `Dto`, `Configuration`, `Exception`

| Elemento | Convencion | Ejemplo |
|---|---|---|
| Clases de entidad | Igual que la tabla DB (singular) | `OrdenServicio`, `Cliente` |
| Propiedades | Igual que la columna DB | `OrdenServicioID`, `FechaIngreso` |
| Interfaces | `I` + PascalCase espannol | `IRepositorioOrden`, `IUnidadTrabajo` |
| Metodos | PascalCase espannol | `ObtenerPorIdAsync`, `AgregarAsync` |
| Variables locales | camelCase espannol | `ordenActual`, `totalRepuestos` |
| Carpetas de features | PascalCase espannol | `CrearOrden/`, `ObtenerOrdenPorId/` |

---

## 1. Estructura de la Solucion

```text
CarFix.sln
├── Dominio/            → CarFix.Dominio           # Interfaces, excepciones y contratos
├── Aplicacion/         → CarFix.Aplicacion        # Casos de uso (CQRS) — depende solo de Dominio
├── Infraestructura/    → CarFix.Infraestructura   # EF Core DB First, repositorios, JWT, servicios externos
├── WebApi/             → CarFix.WebApi            # Endpoints HTTP, autenticacion, configuracion del host
└── Especificaciones/   → CarFix.Especificaciones  # Specs ejecutables Gherkin con Reqnroll
```

**Regla de oro:** Dominio no conoce a nadie. Aplicacion no conoce a Infraestructura ni a WebApi. Infraestructura no conoce a WebApi. Especificaciones conoce a Aplicacion e Infraestructura (solo para pruebas).

---

## 2. Entity Framework Core — Database First

### Enfoque

Las entidades **se generan automaticamente desde la base de datos** con el comando `scaffold`. **Nunca se crean a mano.** Al cambiar el esquema de BD, se regeneran.

Los archivos generados van a `Infraestructura/Persistencia/Generado/` y **nunca se editan directamente**. El comportamiento de dominio se agrega mediante **clases parciales** en `Infraestructura/Persistencia/Extensiones/`.

### Comando de Scaffolding

```bash
dotnet ef dbcontext scaffold \
  "Server=localhost\SQL2022;Database=CAR_FIX;Integrated Security=True;TrustServerCertificate=True" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Persistencia/Generado \
  --context-dir Persistencia \
  --context CarFixDbContext \
  --namespace CarFix.Infraestructura.Persistencia.Generado \
  --context-namespace CarFix.Infraestructura.Persistencia \
  --force \
  --no-onconfiguring
  --project CarFix.Infraestructura
```

Opciones clave:
- `--force`: sobreescribe archivos existentes en cada regeneracion
- `--no-onconfiguring`: evita que la cadena de conexion quede hardcodeada en el contexto generado
- `--output-dir Persistencia/Generado`: las entidades generadas van aqui, separadas del codigo propio

### Estructura generada

```text
CarFix.Infraestructura/
└── Persistencia/
    ├── CarFixDbContext.cs          # Generado — no editar
    └── Generado/
        ├── Cliente.cs              # Generado — no editar
        ├── Vehiculo.cs             # Generado — no editar
        ├── OrdenServicio.cs        # Generado — no editar
        ├── Reparacion.cs           # Generado — no editar
        ├── Repuesto.cs             # Generado — no editar
        ├── Factura.cs              # Generado — no editar
        ├── Taller.cs               # Generado — no editar
        ├── TipoReparacion.cs       # Generado — no editar
        ├── EstadoOrden.cs          # Generado — no editar
        ├── EstadoFactura.cs        # Generado — no editar
        ├── Rol.cs                  # Generado — no editar
        └── Usuario.cs              # Generado — no editar
```

### Extender entidades con clases parciales

Para agregar comportamiento de dominio sin tocar los archivos generados, se usan clases parciales:

```csharp
// CarFix.Infraestructura/Persistencia/Extensiones/OrdenServicioExtension.cs
namespace CarFix.Infraestructura.Persistencia.Generado;

public partial class OrdenServicio
{
    public static OrdenServicio Crear(int vehiculoId, int facturaId, string problemaGeneral, int estadoInicialId)
    {
        if (string.IsNullOrWhiteSpace(problemaGeneral))
            throw new ExcepcionDominio("El problema general es requerido.");

        return new OrdenServicio
        {
            VehiculoID      = vehiculoId,
            FacturaID       = facturaId,
            ProblemaGeneral = problemaGeneral,
            EstadoOrdenID   = estadoInicialId,
            FechaIngreso    = DateTime.UtcNow,
            FechaSalida     = DateTime.UtcNow.AddDays(3),
            EsGarantia      = false
        };
    }

    public void CambiarEstado(int nuevoEstadoId)
        => EstadoOrdenID = nuevoEstadoId;
}
```

### Extender el DbContext con clase parcial

El DbContext generado se extiende para implementar la interfaz `ICarFixDbContext` sin tocar el archivo generado:

```csharp
// CarFix.Infraestructura/Persistencia/CarFixDbContextExtension.cs
namespace CarFix.Infraestructura.Persistencia;

public partial class CarFixDbContext : ICarFixDbContext
{
    public async Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        => await base.SaveChangesAsync(ct);
}
```

---

## 3. Capa de Dominio (`CarFix.Dominio`)

Con Database First, esta capa contiene **solo contratos y excepciones**. Las entidades viven en Infrastructure (generadas por EF).

```text
CarFix.Dominio/
├── Interfaces/
│   ├── ICarFixDbContext.cs
│   ├── IRepositorioOrdenServicio.cs
│   ├── IRepositorioCliente.cs
│   ├── IServicioToken.cs
│   ├── IServicioContrasenna.cs
│   └── IUnidadTrabajo.cs
└── Excepciones/
    └── ExcepcionDominio.cs
```

### Interfaz del DbContext (para queries directas)

```csharp
// CarFix.Dominio/Interfaces/ICarFixDbContext.cs
public interface ICarFixDbContext
{
    DbSet<Cliente>       Clientes        { get; }
    DbSet<Vehiculo>      Vehiculos       { get; }
    DbSet<OrdenServicio> OrdenesServicio { get; }
    DbSet<Reparacion>    Reparaciones    { get; }
    DbSet<Repuesto>      Repuestos       { get; }
    DbSet<Factura>       Facturas        { get; }
    DbSet<Rol>           Roles           { get; }
    DbSet<Usuario>       Usuarios        { get; }

    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
```

### Interfaces de Repositorio

```csharp
// CarFix.Dominio/Interfaces/IRepositorioOrdenServicio.cs
public interface IRepositorioOrdenServicio
{
    Task<OrdenServicio?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<OrdenServicio>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                             AgregarAsync(OrdenServicio orden, CancellationToken ct = default);
    void                             Actualizar(OrdenServicio orden);
}
```

### Servicio de Contrasenna

```csharp
// CarFix.Dominio/Interfaces/IServicioContrasenna.cs
public interface IServicioContrasenna
{
    bool   Verificar(string contrasenna, string hash);
    string Hashear(string contrasenna);
}
```

### Excepcion de Dominio

```csharp
// CarFix.Dominio/Excepciones/ExcepcionDominio.cs
public class ExcepcionDominio : Exception
{
    public ExcepcionDominio(string mensaje) : base(mensaje) { }
}
```

---

## 4. Capa de Aplicacion (`CarFix.Aplicacion`)

Orquesta los casos de uso con **CQRS y MediatR**. Depende solo de `CarFix.Dominio`.

**Paquetes NuGet:**
- `MediatR`
- `FluentValidation`
- `FluentValidation.DependencyInjectionExtensions`

### Patron Resultado

```csharp
// CarFix.Aplicacion/Comun/Resultado.cs
public class Resultado<T>
{
    public bool    EsExitoso { get; }
    public T?      Valor     { get; }
    public string? Error     { get; }

    private Resultado(T valor)      { EsExitoso = true;  Valor = valor; }
    private Resultado(string error) { EsExitoso = false; Error = error; }

    public static Resultado<T> Exito(T valor)      => new(valor);
    public static Resultado<T> Fallo(string error) => new(error);
}

public class Resultado
{
    public bool    EsExitoso { get; }
    public string? Error     { get; }

    private Resultado(bool esExitoso, string? error) { EsExitoso = esExitoso; Error = error; }

    public static Resultado Exito()                => new(true, null);
    public static Resultado Fallo(string error)    => new(false, error);
}
```

### Autenticacion — IniciarSesion

El handler verifica credenciales contra `Catalogo.Usuarios`, valida el hash con BCrypt via `IServicioContrasenna` y genera el JWT via `IServicioToken`.

```csharp
// CarFix.Aplicacion/Features/Autenticacion/Commands/IniciarSesion/RespuestaTokenDto.cs
public record RespuestaTokenDto(string Token, DateTime Expiracion);

// CarFix.Aplicacion/Features/Autenticacion/Commands/IniciarSesion/IniciarSesionCommand.cs
public record IniciarSesionCommand(
    string NombreUsuario,
    string Password
) : IRequest<Resultado<RespuestaTokenDto>>;

// CarFix.Aplicacion/Features/Autenticacion/Commands/IniciarSesion/IniciarSesionHandler.cs
public class IniciarSesionHandler : IRequestHandler<IniciarSesionCommand, Resultado<RespuestaTokenDto>>
{
    private readonly ICarFixDbContext     _contexto;
    private readonly IServicioToken       _servicioToken;
    private readonly IServicioContrasenna _servicioContrasenna;

    public IniciarSesionHandler(
        ICarFixDbContext     contexto,
        IServicioToken       servicioToken,
        IServicioContrasenna servicioContrasenna)
    {
        _contexto            = contexto;
        _servicioToken       = servicioToken;
        _servicioContrasenna = servicioContrasenna;
    }

    public async Task<Resultado<RespuestaTokenDto>> Handle(IniciarSesionCommand cmd, CancellationToken ct)
    {
        var usuario = await _contexto.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == cmd.NombreUsuario && u.Activo, ct);

        if (usuario is null || !_servicioContrasenna.Verificar(cmd.Password, usuario.PasswordHash))
            return Resultado<RespuestaTokenDto>.Fallo("Credenciales invalidas.");

        var expiracion = DateTime.UtcNow.AddMinutes(60);
        var token      = _servicioToken.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol.Nombre);

        return Resultado<RespuestaTokenDto>.Exito(new RespuestaTokenDto(token, expiracion));
    }
}

// CarFix.Aplicacion/Features/Autenticacion/Commands/IniciarSesion/IniciarSesionValidator.cs
public class IniciarSesionValidator : AbstractValidator<IniciarSesionCommand>
{
    public IniciarSesionValidator()
    {
        RuleFor(x => x.NombreUsuario).NotEmpty().WithMessage("El nombre de usuario es requerido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contrasenna es requerida.");
    }
}
```

### Comando

```csharp
// CarFix.Aplicacion/Features/OrdenesServicio/Commands/CrearOrden/CrearOrdenCommand.cs
public record CrearOrdenCommand(
    int    VehiculoID,
    int    FacturaID,
    string ProblemaGeneral
) : IRequest<Resultado<int>>;
```

### Handler del Comando

```csharp
// CarFix.Aplicacion/Features/OrdenesServicio/Commands/CrearOrden/CrearOrdenHandler.cs
public class CrearOrdenHandler : IRequestHandler<CrearOrdenCommand, Resultado<int>>
{
    private readonly IRepositorioOrdenServicio _repositorio;
    private readonly IUnidadTrabajo            _unidadTrabajo;

    public CrearOrdenHandler(IRepositorioOrdenServicio repositorio, IUnidadTrabajo unidadTrabajo)
    {
        _repositorio   = repositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearOrdenCommand cmd, CancellationToken ct)
    {
        const int estadoRecibidoId = 2; // ID 2 = Recibido en Catalogo.EstadoOrden

        var orden = OrdenServicio.Crear(cmd.VehiculoID, cmd.FacturaID, cmd.ProblemaGeneral, estadoRecibidoId);

        await _repositorio.AgregarAsync(orden, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(orden.OrdenServicioID);
    }
}
```

### Validador

```csharp
// CarFix.Aplicacion/Features/OrdenesServicio/Commands/CrearOrden/CrearOrdenValidator.cs
public class CrearOrdenValidator : AbstractValidator<CrearOrdenCommand>
{
    public CrearOrdenValidator()
    {
        RuleFor(x => x.VehiculoID)
            .GreaterThan(0).WithMessage("El vehiculo es requerido.");

        RuleFor(x => x.ProblemaGeneral)
            .NotEmpty().WithMessage("Debe describir el problema.")
            .MaximumLength(200);
    }
}
```

### Query (Lectura directa al DbContext)

```csharp
// CarFix.Aplicacion/Features/OrdenesServicio/Queries/ObtenerOrdenPorId/ObtenerOrdenPorIdQuery.cs
public record ObtenerOrdenPorIdQuery(int Id) : IRequest<Resultado<OrdenServicioDto>>;

// CarFix.Aplicacion/Features/OrdenesServicio/Queries/ObtenerOrdenPorId/ObtenerOrdenPorIdHandler.cs
public class ObtenerOrdenPorIdHandler : IRequestHandler<ObtenerOrdenPorIdQuery, Resultado<OrdenServicioDto>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerOrdenPorIdHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<OrdenServicioDto>> Handle(ObtenerOrdenPorIdQuery query, CancellationToken ct)
    {
        var dto = await _contexto.OrdenesServicio
            .Where(o => o.OrdenServicioID == query.Id)
            .Select(o => new OrdenServicioDto(
                o.OrdenServicioID,
                o.ProblemaGeneral,
                o.FechaIngreso,
                o.EstadoOrdenID))
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return Resultado<OrdenServicioDto>.Fallo("Orden no encontrada.");

        return Resultado<OrdenServicioDto>.Exito(dto);
    }
}
```

### Registro de Servicios de Application

```csharp
// CarFix.Aplicacion/InjeccionDependencias.cs
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
```

---

## 5. Capa de Infraestructura (`CarFix.Infraestructura`)

**Paquetes NuGet:**
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `BCrypt.Net-Next`

### Repositorio

```csharp
// CarFix.Infraestructura/Persistencia/Repositorios/RepositorioOrdenServicio.cs
public class RepositorioOrdenServicio : IRepositorioOrdenServicio
{
    private readonly CarFixDbContext _contexto;

    public RepositorioOrdenServicio(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<OrdenServicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.OrdenesServicio
                          .Include(o => o.Reparaciones)
                          .FirstOrDefaultAsync(o => o.OrdenServicioID == id, ct);

    public async Task<IEnumerable<OrdenServicio>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.OrdenesServicio.ToListAsync(ct);

    public async Task AgregarAsync(OrdenServicio orden, CancellationToken ct = default)
        => await _contexto.OrdenesServicio.AddAsync(orden, ct);

    public void Actualizar(OrdenServicio orden)
        => _contexto.OrdenesServicio.Update(orden);
}
```

### Unidad de Trabajo

```csharp
// CarFix.Dominio/Interfaces/IUnidadTrabajo.cs
public interface IUnidadTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}

// CarFix.Infraestructura/Persistencia/UnidadTrabajo.cs
public class UnidadTrabajo : IUnidadTrabajo
{
    private readonly CarFixDbContext _contexto;
    public UnidadTrabajo(CarFixDbContext contexto) => _contexto = contexto;

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        => _contexto.SaveChangesAsync(ct);
}
```

### Servicio de Token JWT

```csharp
// CarFix.Dominio/Interfaces/IServicioToken.cs
public interface IServicioToken
{
    string GenerarToken(int usuarioId, string nombreUsuario, string rol);
}

// CarFix.Infraestructura/Seguridad/ServicioToken.cs
public class ServicioToken : IServicioToken
{
    private readonly IConfiguration _config;

    public ServicioToken(IConfiguration config) => _config = config;

    public string GenerarToken(int usuarioId, string nombreUsuario, string rol)
    {
        var llave        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Llave"]!));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var reclamaciones = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Name,            nombreUsuario),
            new Claim(ClaimTypes.Role,            rol)
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Emisor"],
            audience:           _config["Jwt:Audiencia"],
            claims:             reclamaciones,
            expires:            DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiracionMinutos"]!)),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Servicio de Contrasenna (BCrypt)

```csharp
// CarFix.Infraestructura/Seguridad/ServicioContrasenna.cs
public class ServicioContrasenna : IServicioContrasenna
{
    public bool   Verificar(string contrasenna, string hash) => BCrypt.Net.BCrypt.Verify(contrasenna, hash);
    public string Hashear(string contrasenna)                => BCrypt.Net.BCrypt.HashPassword(contrasenna);
}
```

### Registro de Servicios de Infrastructure

```csharp
// CarFix.Infraestructura/InjeccionDependencias.cs
public static class InjeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration config)
    {
        // Base de datos
        services.AddDbContext<CarFixDbContext>(opciones =>
            opciones.UseSqlServer(config.GetConnectionString("CarFix")));

        services.AddScoped<ICarFixDbContext>(sp => sp.GetRequiredService<CarFixDbContext>());
        services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
        services.AddScoped<IRepositorioOrdenServicio, RepositorioOrdenServicio>();

        // Seguridad
        services.AddScoped<IServicioToken,       ServicioToken>();
        services.AddScoped<IServicioContrasenna, ServicioContrasenna>();

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
```

---

## 6. Capa de API (`CarFix.WebApi`)

### Endpoint de Autenticacion (Login)

```csharp
// CarFix.WebApi/Endpoints/EndpointsAutenticacion.cs
public static class EndpointsAutenticacion
{
    public static void MapearAutenticacion(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/autenticacion")
                       .WithTags("Autenticacion")
                       .WithOpenApi()
                       .AllowAnonymous();

        grupo.MapPost("/iniciar-sesion", async Task<Results<Ok<RespuestaTokenDto>, UnauthorizedHttpResult>>
            (IniciarSesionCommand cmd, ISender sender) =>
        {
            var resultado = await sender.Send(cmd);
            return resultado.EsExitoso
                ? TypedResults.Ok(resultado.Valor)
                : TypedResults.Unauthorized();
        })
        .WithName("IniciarSesion")
        .WithSummary("Genera un token JWT para acceder a los endpoints protegidos");
    }
}
```

> `RespuestaTokenDto` se define en `CarFix.Aplicacion/Features/Autenticacion/Commands/IniciarSesion/`.

### Endpoints Protegidos con JWT

```csharp
// CarFix.WebApi/Endpoints/EndpointsOrdenes.cs
public static class EndpointsOrdenes
{
    public static void MapearOrdenes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/ordenes")
                       .WithTags("Ordenes")
                       .WithOpenApi()
                       .RequireAuthorization(); // Protege todo el grupo con JWT

        grupo.MapGet("{id:int}", async Task<Results<Ok<OrdenServicioDto>, NotFound<string>>>
            (int id, ISender sender) =>
        {
            var resultado = await sender.Send(new ObtenerOrdenPorIdQuery(id));
            return resultado.EsExitoso
                ? TypedResults.Ok(resultado.Valor)
                : TypedResults.NotFound(resultado.Error);
        })
        .WithName("ObtenerOrdenPorId");

        grupo.MapPost("/", async Task<Results<Created<int>, BadRequest<string>>>
            (CrearOrdenCommand cmd, ISender sender) =>
        {
            var resultado = await sender.Send(cmd);
            return resultado.EsExitoso
                ? TypedResults.Created($"/api/ordenes/{resultado.Valor}", resultado.Valor)
                : TypedResults.BadRequest(resultado.Error);
        })
        .WithName("CrearOrden");
    }
}
```

### Manejador de Excepciones Global

```csharp
// CarFix.WebApi/Excepciones/ManejadorExcepciones.cs
public class ManejadorExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorExcepciones> _logger;

    public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext contextoHttp, Exception excepcion, CancellationToken ct)
    {
        var (codigoEstado, titulo, detalle) = excepcion switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Error de validacion",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),

            ExcepcionDominio ex => (
                StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio violada",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrio un error inesperado.")
        };

        if (codigoEstado == 500)
            _logger.LogError(excepcion, "Excepcion no controlada");

        contextoHttp.Response.StatusCode = codigoEstado;
        await contextoHttp.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = codigoEstado,
            Title  = titulo,
            Detail = detalle
        }, ct);

        return true;
    }
}
```

### Program.cs

```csharp
// CarFix.WebApi/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AgregarAplicacion()
    .AgregarInfraestructura(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            Description  = "Ingresa el token JWT obtenido desde /api/autenticacion/iniciar-sesion"
        };

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.MapearAutenticacion();
app.MapearOrdenes();
// app.MapearClientes();
// app.MapearFacturas();

app.Run();
```

### Configuracion (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "CarFix": "Server=localhost\\SQL2022;Database=CAR_FIX;Integrated Security=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Llave": "clave-secreta-minimo-32-caracteres-aqui",
    "Emisor": "CarFix.WebApi",
    "Audiencia": "CarFix.Cliente",
    "ExpiracionMinutos": "60"
  }
}
```

> La `Jwt:Llave` nunca debe ir en el repositorio. Usar `dotnet user-secrets` en desarrollo y variables de entorno en produccion.

---

## 7. Patrones y Reglas Transversales

### Estructura de carpetas completa

```text
CarFix.sln
│
├── Dominio/                            ← carpeta fisica en disco
│   └── CarFix.Dominio.csproj
│       ├── Excepciones/
│       │   └── ExcepcionDominio.cs
│       └── Interfaces/
│           ├── ICarFixDbContext.cs
│           ├── IUnidadTrabajo.cs
│           ├── IServicioToken.cs
│           ├── IServicioContrasenna.cs
│           └── IRepositorioOrdenServicio.cs
│
├── Aplicacion/                         ← carpeta fisica en disco
│   └── CarFix.Aplicacion.csproj
│       ├── Comun/
│       │   ├── Comportamientos/
│       │   │   └── ComportamientoValidacion.cs
│       │   └── Resultado.cs
│       ├── InjeccionDependencias.cs
│       └── Features/
│           ├── Autenticacion/
│           │   └── Commands/IniciarSesion/
│           │       ├── IniciarSesionCommand.cs
│           │       ├── IniciarSesionHandler.cs
│           │       ├── IniciarSesionValidator.cs
│           │       └── RespuestaTokenDto.cs
│           ├── OrdenesServicio/
│           │   ├── Commands/CrearOrden/
│           │   └── Queries/ObtenerOrdenPorId/
│           ├── Clientes/
│           ├── Facturas/
│           └── Repuestos/
│
├── Infraestructura/                    ← carpeta fisica en disco
│   └── CarFix.Infraestructura.csproj
│       ├── InjeccionDependencias.cs
│       ├── Persistencia/
│       │   ├── CarFixDbContext.cs          # Generado por EF — no editar
│       │   ├── CarFixDbContextExtension.cs # Clase parcial propia
│       │   ├── Generado/                   # Entidades generadas por EF — no editar
│       │   ├── Extensiones/                # Clases parciales con comportamiento
│       │   ├── Repositorios/
│       │   │   └── RepositorioOrdenServicio.cs
│       │   └── UnidadTrabajo.cs
│       └── Seguridad/
│           ├── ServicioToken.cs
│           └── ServicioContrasenna.cs
│
└── WebApi/                             ← carpeta fisica en disco
    └── CarFix.WebApi.csproj
        ├── Program.cs
        ├── Endpoints/
        │   ├── EndpointsAutenticacion.cs
        │   ├── EndpointsOrdenes.cs
        │   └── EndpointsClientes.cs
        └── Excepciones/
            └── ManejadorExcepciones.cs
```

### Evitar (Anti-patrones)

| Anti-patron | Correccion |
|---|---|
| Editar archivos en `Persistencia/Generado/` | Usar clases parciales en `Persistencia/Extensiones/` |
| Crear entidades a mano (Code First) | Siempre usar scaffold desde la BD |
| Logica de negocio en endpoints | Mover al Handler o a la clase parcial de la entidad |
| Exponer el DbContext fuera de Infrastructure | Inyectar `ICarFixDbContext` (interfaz) en queries |
| Guardar la llave JWT en `appsettings.json` en produccion | Usar variables de entorno o secrets manager |
| Endpoints sin `RequireAuthorization()` | Todo endpoint (excepto login) debe estar protegido |
| Guardar contrasenna en texto plano | Siempre usar `IServicioContrasenna.Hashear()` al crear usuarios |
| Verificar contrasenna en el endpoint | Delegar al `IniciarSesionHandler` via MediatR |

---

## 8. Especificaciones — Spec Driven Development (Reqnroll)

Las especificaciones describen el comportamiento del sistema en lenguaje de negocio **antes** de escribir el codigo. Cada spec es un escenario ejecutable que actua como documentacion viva y prueba de aceptacion al mismo tiempo.

> **Herramienta:** Reqnroll (fork activo de SpecFlow, compatible con .NET 10). No usar SpecFlow — fue discontinuado en 2024.

### Proyecto y paquetes

```bash
# Crear el proyecto de especificaciones dentro de la carpeta Especificaciones/
dotnet new nunit -n CarFix.Especificaciones -o Especificaciones/CarFix.Especificaciones
dotnet add Especificaciones/CarFix.Especificaciones package Reqnroll.NUnit
dotnet add Especificaciones/CarFix.Especificaciones package Reqnroll.Microsoft.Extensions.DependencyInjection
dotnet add Especificaciones/CarFix.Especificaciones package Microsoft.EntityFrameworkCore.InMemory
```

### Alcance

Las specs cubren la **capa de Aplicacion** (handlers de MediatR). Prueban las reglas de negocio sin depender del transporte HTTP. La BD se reemplaza por un proveedor en memoria (`UseInMemoryDatabase`) para que las pruebas sean rapidas y sin efectos secundarios.

### Convencion de archivos `.feature`

- Un archivo `.feature` por entidad o caso de uso principal
- Nombre en espannol, sin caracteres especiales: `OrdenesServicio.feature`
- Cada `Scenario` describe un caso de negocio completo con datos concretos
- Usar `Scenario Outline` + `Examples` cuando el mismo flujo cambia solo los datos

### Ejemplo completo — Autenticacion

```gherkin
# Especificaciones/CarFix.Especificaciones/Features/Autenticacion.feature
Feature: Autenticacion de usuarios
  Como usuario del sistema
  Quiero iniciar sesion con mis credenciales
  Para obtener acceso al sistema

  Background:
    Given que existe un rol "Mecanico" con ID 3
    And que existe un usuario "jperez" con contrasenna "Taller2024!" y rol "Mecanico"

  Scenario: Inicio de sesion exitoso
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    Then recibe un token JWT valido

  Scenario: Contrasenna incorrecta
    When el usuario inicia sesion con "jperez" y "incorrecta"
    Then el sistema rechaza el acceso con "Credenciales invalidas."

  Scenario: Usuario inexistente
    When el usuario inicia sesion con "noexiste" y "cualquiera"
    Then el sistema rechaza el acceso con "Credenciales invalidas."

  Scenario: Usuario inactivo
    Given que el usuario "jperez" esta desactivado
    When el usuario inicia sesion con "jperez" y "Taller2024!"
    Then el sistema rechaza el acceso con "Credenciales invalidas."
```

### Step Definitions

```csharp
// Especificaciones/CarFix.Especificaciones/Steps/AutenticacionSteps.cs
[Binding]
public class AutenticacionSteps
{
    private readonly ISender             _sender;
    private readonly ICarFixDbContext    _contexto;
    private readonly IServicioContrasenna _servicioContrasenna;
    private Resultado<RespuestaTokenDto> _ultimoResultado;

    public AutenticacionSteps(ISender sender, ICarFixDbContext contexto, IServicioContrasenna servicioContrasenna)
    {
        _sender              = sender;
        _contexto            = contexto;
        _servicioContrasenna = servicioContrasenna;
    }

    [Given(@"que existe un rol ""(.*)"" con ID (\d+)")]
    public async Task DadoExisteRol(string nombre, int rolId)
    {
        _contexto.Roles.Add(new Rol { RolID = rolId, Nombre = nombre });
        await _contexto.GuardarCambiosAsync();
    }

    [Given(@"que existe un usuario ""(.*)"" con contrasenna ""(.*)"" y rol ""(.*)""")]
    public async Task DadoExisteUsuario(string nombreUsuario, string contrasenna, string nombreRol)
    {
        var rol = await _contexto.Roles.FirstAsync(r => r.Nombre == nombreRol);
        _contexto.Usuarios.Add(new Usuario
        {
            NombreUsuario  = nombreUsuario,
            PasswordHash   = _servicioContrasenna.Hashear(contrasenna),
            NombreCompleto = "Usuario de prueba",
            Activo         = true,
            RolID          = rol.RolID
        });
        await _contexto.GuardarCambiosAsync();
    }

    [Given(@"que el usuario ""(.*)"" esta desactivado")]
    public async Task DadoUsuarioDesactivado(string nombreUsuario)
    {
        var usuario = await _contexto.Usuarios.FirstAsync(u => u.NombreUsuario == nombreUsuario);
        usuario.Activo = false;
        await _contexto.GuardarCambiosAsync();
    }

    [When(@"el usuario inicia sesion con ""(.*)"" y ""(.*)""")]
    public async Task CuandoIniciaSesion(string nombreUsuario, string password)
    {
        _ultimoResultado = await _sender.Send(new IniciarSesionCommand(nombreUsuario, password));
    }

    [Then(@"recibe un token JWT valido")]
    public void EntoncesTokenValido()
        => Assert.That(_ultimoResultado.EsExitoso, Is.True);

    [Then(@"el sistema rechaza el acceso con ""(.*)""")]
    public void EntoncesRechazo(string mensaje)
    {
        Assert.That(_ultimoResultado.EsExitoso, Is.False);
        Assert.That(_ultimoResultado.Error, Is.EqualTo(mensaje));
    }
}
```

### Configuracion de DI para las especificaciones

```csharp
// Especificaciones/CarFix.Especificaciones/Soporte/ConfiguracionEspecificaciones.cs
[Binding]
public class ConfiguracionEspecificaciones
{
    private static IServiceProvider _proveedor;

    [BeforeTestRun]
    public static void AntesDeTodo()
    {
        var servicios = new ServiceCollection();

        servicios.AddDbContext<CarFixDbContext>(op =>
            op.UseInMemoryDatabase("CarFixTest"));

        servicios.AddScoped<ICarFixDbContext>(sp => sp.GetRequiredService<CarFixDbContext>());
        servicios.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
        servicios.AddScoped<IRepositorioOrdenServicio, RepositorioOrdenServicio>();
        servicios.AddScoped<IServicioContrasenna, ServicioContrasenna>();
        servicios.AddScoped<IServicioToken, ServicioTokenFalso>(); // stub para tests
        servicios.AgregarAplicacion();

        _proveedor = servicios.BuildServiceProvider();
    }

    [BeforeScenario]
    public void AntesDeCadaEscenario(IServiceScope scope)
    {
        var contexto = scope.ServiceProvider.GetRequiredService<CarFixDbContext>();
        contexto.Database.EnsureDeleted();
        contexto.Database.EnsureCreated();

        contexto.EstadosOrden.AddRange(
            new EstadoOrden { EstadoOrdenID = 1, Descripcion = "Cotizacion" },
            new EstadoOrden { EstadoOrdenID = 2, Descripcion = "Recibido" },
            new EstadoOrden { EstadoOrdenID = 3, Descripcion = "En reparacion" },
            new EstadoOrden { EstadoOrdenID = 4, Descripcion = "Finalizado" },
            new EstadoOrden { EstadoOrdenID = 5, Descripcion = "Entregado" }
        );
        contexto.SaveChanges();
    }
}

// Stub de IServicioToken para pruebas — no genera JWT real
public class ServicioTokenFalso : IServicioToken
{
    public string GenerarToken(int usuarioId, string nombreUsuario, string rol)
        => $"token-falso-{usuarioId}-{rol}";
}
```

### Estructura del proyecto de especificaciones

```text
Especificaciones/                       ← carpeta fisica en disco
└── CarFix.Especificaciones.csproj
    ├── Features/
    │   ├── Autenticacion.feature
    │   ├── OrdenesServicio.feature
    │   ├── Clientes.feature
    │   ├── Repuestos.feature
    │   └── Facturas.feature
    ├── Steps/
    │   ├── AutenticacionSteps.cs
    │   ├── OrdenServicioSteps.cs
    │   ├── ClienteSteps.cs
    │   └── FacturaSteps.cs
    └── Soporte/
        └── ConfiguracionEspecificaciones.cs
```

### Flujo de trabajo con SDD

```
1. Escribir el escenario en el .feature (negocio primero)
        ↓
2. Ejecutar → Reqnroll reporta steps sin implementar
        ↓
3. Implementar los Step Definitions
        ↓
4. Ejecutar → falla (no existe logica aun)
        ↓
5. Implementar Command/Query + Handler en Aplicacion
        ↓
6. Ejecutar → pasa
        ↓
7. Refactorizar y pasar al siguiente escenario
```

---

## 9. Resumen de Capas (Cheat Sheet)

| Capa | Carpeta / Proyecto | Responsabilidad | Conoce a |
|---|---|---|---|
| **Dominio** | `Dominio/CarFix.Dominio` | Interfaces de repositorio, `IUnidadTrabajo`, `IServicioToken`, `IServicioContrasenna`, `ExcepcionDominio` | Nadie |
| **Aplicacion** | `Aplicacion/CarFix.Aplicacion` | Casos de uso CQRS, `Resultado<T>`, DTOs, validaciones, `IniciarSesionHandler` | Dominio |
| **Infraestructura** | `Infraestructura/CarFix.Infraestructura` | EF Core DB First, repositorios, `UnidadTrabajo`, `ServicioToken`, `ServicioContrasenna`, JWT | Aplicacion + Dominio |
| **WebApi** | `WebApi/CarFix.WebApi` | Endpoints HTTP, auth JWT, `ManejadorExcepciones`, `Program.cs` | Aplicacion + Infraestructura |
| **Especificaciones** | `Especificaciones/CarFix.Especificaciones` | Specs ejecutables Gherkin + Reqnroll, BD en memoria | Aplicacion + Infraestructura |
