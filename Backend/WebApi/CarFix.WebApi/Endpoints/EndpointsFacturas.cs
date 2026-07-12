using CarFix.Aplicacion.Features.Facturas.Commands.ActualizarFactura;
using CarFix.Aplicacion.Features.Facturas.Commands.CambiarEstadoFactura;
using CarFix.Aplicacion.Features.Facturas.Commands.CrearFactura;
using CarFix.Aplicacion.Features.Facturas.Commands.EliminarFactura;
using CarFix.Aplicacion.Features.Facturas.Commands.EnviarFactura;
using CarFix.Aplicacion.Features.Facturas.Dtos;
using CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturaPdf;
using CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturas;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsFacturas
{
    public static void MapearFacturas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/facturas")
            .WithTags("Facturas")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<FacturaDto>>>
            (string? filtro, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerFacturasQuery(filtro));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerFacturas")
            .WithSummary("Lista de facturas con filtro opcional");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearFacturaRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new CrearFacturaCommand(
                    req.VehiculoId, req.Fecha, req.DescripcionGeneral,
                    req.Descuento, req.Adelanto, req.ImpuestoVentas));
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/facturas/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearFactura")
            .WithSummary("Crea una nueva factura en estado Cotizacion");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarFacturaRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new ActualizarFacturaCommand(
                    id, req.Fecha, req.DescripcionGeneral, req.Descuento, req.Adelanto, req.ImpuestoVentas));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarFactura")
            .WithSummary("Actualiza campos editables de una factura (no Pagada)");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarFacturaCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarFactura")
            .WithSummary("Elimina una factura en estado Cotizacion junto con su orden y detalle");

        grupo.MapPatch("/{id:int}/estado",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, CambiarEstadoRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new CambiarEstadoFacturaCommand(id, req.NuevoEstadoId));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CambiarEstadoFactura")
            .WithSummary("Cambia el estado de una factura (1=Cotizacion, 2=Pendiente, 3=Pagada)");

        grupo.MapPost("/{id:int}/enviar",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EnviarFacturaCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EnviarFactura")
            .WithSummary("Genera el PDF de la factura y lo envia por correo al email del cliente");

        grupo.MapGet("/{id:int}/pdf",
            async Task<Results<FileContentHttpResult, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerFacturaPdfQuery(id));
                return resultado.EsExitoso
                    ? TypedResults.File(resultado.Valor!, "application/pdf", $"Factura-{id:D4}.pdf")
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ObtenerFacturaPdf")
            .WithSummary("Genera y devuelve el PDF de la factura para verlo o descargarlo");
    }
}

file record CrearFacturaRequest(
    int      VehiculoId,
    DateTime Fecha,
    string   DescripcionGeneral,
    decimal  Descuento,
    decimal  Adelanto,
    decimal  ImpuestoVentas);

file record ActualizarFacturaRequest(
    DateTime Fecha,
    string?  DescripcionGeneral,
    decimal  Descuento,
    decimal  Adelanto,
    decimal  ImpuestoVentas);

file record CambiarEstadoRequest(int NuevoEstadoId);
