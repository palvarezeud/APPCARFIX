using CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsAutenticacion
{
    public static void MapearAutenticacion(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/autenticacion")
            .WithTags("Autenticacion")
            .AllowAnonymous();

        grupo.MapPost("/iniciar-sesion",
            async Task<Results<Ok<RespuestaTokenDto>, UnauthorizedHttpResult>>
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
