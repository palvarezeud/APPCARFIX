using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CarFix.WebApi.OpenApi;

public class TransformadorSeguridadJwt : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument                  document,
        OpenApiDocumentTransformerContext context,
        CancellationToken                cancellationToken)
    {
        var componentes = document.Components ??= new OpenApiComponents();
        var esquemas    = componentes.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        esquemas["Bearer"] = new OpenApiSecurityScheme
        {
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            Description  = "Ingresa el token JWT obtenido desde /api/autenticacion/iniciar-sesion"
        };

        return Task.CompletedTask;
    }
}
