using CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;
using CarFix.Aplicacion.Features.Autenticacion.Commands.RefrescarSesion;
using CarFix.Aplicacion.Features.Autenticacion.Commands.RevocarSesion;
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

        grupo.MapPost("/refrescar",
            async Task<Results<Ok<RespuestaTokenDto>, UnauthorizedHttpResult>>
            (RefrescarSesionCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Ok(resultado.Valor)
                    : TypedResults.Unauthorized();
            })
            .WithName("RefrescarSesion")
            .WithSummary("Emite un nuevo JWT a partir de un token de refresco valido (rota el token de refresco)");

        grupo.MapPost("/cerrar-sesion",
            async Task<Ok> (RevocarSesionCommand cmd, ISender sender) =>
            {
                await sender.Send(cmd);
                return TypedResults.Ok();
            })
            .WithName("CerrarSesion")
            .WithSummary("Revoca un token de refresco (cierre de sesion explicito)");
    }
}
