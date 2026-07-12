using CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.ActualizarHistoricoRepuesto;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.CrearHistoricoRepuesto;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.EliminarHistoricoRepuesto;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Dtos;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Queries.ObtenerHistoricoRepuestos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsHistoricoRepuestos
{
    public static void MapearHistoricoRepuestos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/historico-repuestos")
            .WithTags("Historico de repuestos")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<HistoricoRespuestoDto>>>
            (string? marca, string? modelo, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerHistoricoRepuestosQuery(marca, modelo));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerHistoricoRepuestos")
            .WithSummary("Historico de repuestos con filtro opcional por marca y modelo");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearHistoricoRepuestoCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/historico-repuestos/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("CrearHistoricoRepuesto")
            .WithSummary("Agrega un registro al historico de repuestos");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarHistoricoRepuestoRequest req, ISender sender) =>
            {
                var cmd = new ActualizarHistoricoRepuestoCommand(
                    id, req.Marca, req.Modelo, req.Annio,
                    req.RepuestoDecripcion, req.Precio, req.Repuestera, req.FechaCompra);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("ActualizarHistoricoRepuesto")
            .WithSummary("Actualiza un registro del historico de repuestos");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarHistoricoRepuestoCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("EliminarHistoricoRepuesto")
            .WithSummary("Elimina un registro del historico de repuestos");
    }
}

public record ActualizarHistoricoRepuestoRequest(
    string   Marca,
    string   Modelo,
    int      Annio,
    string   RepuestoDecripcion,
    decimal  Precio,
    string   Repuestera,
    DateTime FechaCompra);
