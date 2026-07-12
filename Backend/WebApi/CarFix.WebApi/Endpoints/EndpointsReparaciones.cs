using CarFix.Aplicacion.Features.Reparaciones.Commands.ActualizarReparacion;
using CarFix.Aplicacion.Features.Reparaciones.Commands.AgregarReparacion;
using CarFix.Aplicacion.Features.Reparaciones.Commands.EliminarReparacion;
using CarFix.Aplicacion.Features.Reparaciones.Commands.MarcarListaReparacion;
using CarFix.Aplicacion.Features.Reparaciones.Dtos;
using CarFix.Aplicacion.Features.Reparaciones.Queries.ObtenerReparaciones;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsReparaciones
{
    public static void MapearReparaciones(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/reparaciones")
            .WithTags("Reparaciones")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<ReparacionDto>>>
            (int facturaId, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerReparacionesQuery(facturaId));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerReparaciones")
            .WithSummary("Lista de reparaciones de una factura");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (AgregarReparacionCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/reparaciones/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("AgregarReparacion")
            .WithSummary("Agrega una reparacion a una factura y actualiza los totales");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarReparacionRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(
                    new ActualizarReparacionCommand(id, req.DescripcionReparacion, req.Costo, req.DuracionAproximadaHoras));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarReparacion")
            .WithSummary("Modifica descripcion y costo de una reparacion y recalcula totales");

        grupo.MapPatch("/{id:int}/listo",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, MarcarListaRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new MarcarListaReparacionCommand(id, req.Listo));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("MarcarListaReparacion")
            .WithSummary("Marca o desmarca una reparacion como lista");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarReparacionCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarReparacion")
            .WithSummary("Elimina una reparacion y actualiza los totales de la factura");
    }
}

file record ActualizarReparacionRequest(string DescripcionReparacion, decimal Costo, int? DuracionAproximadaHoras = null);
file record MarcarListaRequest(bool Listo);
