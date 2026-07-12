using CarFix.Aplicacion.Features.Talleres.Commands.ActualizarTaller;
using CarFix.Aplicacion.Features.Talleres.Dtos;
using CarFix.Aplicacion.Features.Talleres.Queries.ObtenerTaller;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsTalleres
{
    public static void MapearTalleres(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/taller")
            .WithTags("Taller")
            .RequireAuthorization(policy => policy.RequireRole("Administrador"));

        grupo.MapGet("/",
            async Task<Results<Ok<TallerDto>, BadRequest<string>>>
            (ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerTallerQuery());
                return resultado.EsExitoso
                    ? TypedResults.Ok(resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ObtenerTaller")
            .WithSummary("Obtiene los datos configurados del taller");

        grupo.MapPut("/",
            async Task<Results<NoContent, BadRequest<string>>>
            (ActualizarTallerCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarTaller")
            .WithSummary("Actualiza los datos del taller (nombre, ubicacion, telefonos, email y geolocalizacion)");
    }
}
