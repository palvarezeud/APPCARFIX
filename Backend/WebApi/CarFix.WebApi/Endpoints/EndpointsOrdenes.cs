using CarFix.Aplicacion.Features.OrdenesServicio.Commands.ActualizarOrden;
using CarFix.Aplicacion.Features.OrdenesServicio.Commands.CambiarEstadoOrden;
using CarFix.Aplicacion.Features.OrdenesServicio.Commands.CrearOrden;
using CarFix.Aplicacion.Features.OrdenesServicio.Commands.EliminarOrden;
using CarFix.Aplicacion.Features.OrdenesServicio.Dtos;
using CarFix.Aplicacion.Features.OrdenesServicio.Queries.ObtenerOrdenes;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsOrdenes
{
    public static void MapearOrdenes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/ordenes")
            .WithTags("Ordenes de servicio")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<OrdenServicioDto>>>
            (string? filtro, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerOrdenesQuery(filtro));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerOrdenes")
            .WithSummary("Lista de ordenes de servicio con filtro opcional por numero de orden o placa");

        grupo.MapPost("/",
            async Task<Results<Created<CrearOrdenResponseDto>, BadRequest<string>>>
            (CrearOrdenCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/ordenes/{resultado.Valor!.OrdenServicioId}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearOrden")
            .WithSummary("Crea una nueva orden de servicio (y su factura en Cotizacion automaticamente)");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarOrdenRequest req, ISender sender) =>
            {
                var cmd = new ActualizarOrdenCommand(
                    id, req.VehiculoId, req.FechaIngreso, req.FechaSalida,
                    req.ProblemaGeneral, req.EsGarantia, req.EstadoOrdenId);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarOrden")
            .WithSummary("Actualiza los datos de una orden de servicio");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarOrdenCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarOrden")
            .WithSummary("Elimina una orden sin repuestos ni reparaciones asociadas");

        grupo.MapPatch("/{id:int}/estado",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, CambiarEstadoRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new CambiarEstadoOrdenCommand(id, req.NuevoEstadoId));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CambiarEstadoOrden")
            .WithSummary("Cambia el estado de una orden de servicio");
    }
}

public record ActualizarOrdenRequest(
    int      VehiculoId,
    DateTime FechaIngreso,
    DateTime FechaSalida,
    string   ProblemaGeneral,
    bool     EsGarantia,
    int      EstadoOrdenId);

file record CambiarEstadoRequest(int NuevoEstadoId);
