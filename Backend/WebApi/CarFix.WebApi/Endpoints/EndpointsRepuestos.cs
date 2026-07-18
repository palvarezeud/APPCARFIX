using CarFix.Aplicacion.Features.Repuestos.Commands.ActualizarRepuesto;
using CarFix.Aplicacion.Features.Repuestos.Commands.AgregarRepuesto;
using CarFix.Aplicacion.Features.Repuestos.Commands.EliminarRepuesto;
using CarFix.Aplicacion.Features.Repuestos.Commands.EscanearFacturaRepuesto;
using CarFix.Aplicacion.Features.Repuestos.Commands.MarcarIncluidoRepuesto;
using CarFix.Aplicacion.Features.Repuestos.Dtos;
using CarFix.Aplicacion.Features.Repuestos.Queries.ObtenerRepuestos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsRepuestos
{
    public static void MapearRepuestos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/repuestos")
            .WithTags("Repuestos")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<RepuestoDto>>>
            (int facturaId, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerRepuestosQuery(facturaId));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerRepuestos")
            .WithSummary("Lista de repuestos de una factura");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (AgregarRepuestoCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/repuestos/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("AgregarRepuesto")
            .WithSummary("Agrega un repuesto a una factura y actualiza los totales");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarRepuestoRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(
                    new ActualizarRepuestoCommand(id, req.NombreRepuesto, req.Costo,
                        req.Fecha, req.Repuestera, req.NumeroFactura));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarRepuesto")
            .WithSummary("Modifica un repuesto y recalcula los totales de la factura");

        grupo.MapPatch("/{id:int}/incluido",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, MarcarIncluidoRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new MarcarIncluidoRepuestoCommand(id, req.Incluido));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("MarcarIncluidoRepuesto")
            .WithSummary("Marca o desmarca un repuesto como incluido/instalado en el vehiculo");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarRepuestoCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarRepuesto")
            .WithSummary("Elimina un repuesto y actualiza los totales de la factura");

        grupo.MapPost("/escanear-factura",
            async Task<Results<Ok<DatosFacturaRepuestoExtraidosDto>, BadRequest<string>>>
            (IFormFile foto, ISender sender) =>
            {
                const long tamannoMaximoBytes = 10 * 1024 * 1024;
                var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };

                if (foto is null || foto.Length == 0)
                    return TypedResults.BadRequest("Debe adjuntar una foto.");
                if (foto.Length > tamannoMaximoBytes)
                    return TypedResults.BadRequest("La imagen no debe superar 10 MB.");
                if (!tiposPermitidos.Contains(foto.ContentType.ToLowerInvariant()))
                    return TypedResults.BadRequest("Formato de imagen no soportado. Use JPEG, PNG o WEBP.");

                await using var stream  = foto.OpenReadStream();
                using var       memoria = new MemoryStream();
                await stream.CopyToAsync(memoria);

                var cmd       = new EscanearFacturaRepuestoCommand(memoria.ToArray(), foto.ContentType);
                var resultado = await sender.Send(cmd);

                return resultado.EsExitoso
                    ? TypedResults.Ok(resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EscanearFacturaRepuesto")
            .WithSummary("Extrae repuestos, monto total, fecha, proveedor y numero de factura de una foto de la factura del proveedor")
            .DisableAntiforgery();
    }
}

file record ActualizarRepuestoRequest(
    string   NombreRepuesto,
    decimal  Costo,
    DateTime Fecha,
    string   Repuestera,
    string?  NumeroFactura);

file record MarcarIncluidoRequest(bool Incluido);
