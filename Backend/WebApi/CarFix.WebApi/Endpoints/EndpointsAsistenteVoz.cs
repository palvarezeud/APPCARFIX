using CarFix.Aplicacion.Features.AsistenteVoz.Commands.InterpretarComandoVoz;
using CarFix.Aplicacion.Features.AsistenteVoz.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsAsistenteVoz
{
    public static void MapearAsistenteVoz(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/asistente-voz")
            .WithTags("AsistenteVoz")
            .RequireAuthorization();

        grupo.MapPost("/interpretar",
            async Task<Results<Ok<InterpretacionVozDto>, BadRequest<string>>>
            (InterpretarComandoVozCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Ok(resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("InterpretarComandoVoz")
            .WithSummary("Interpreta un comando de voz transcrito (navegacion, busqueda o creacion de cliente/vehiculo)");
    }
}
