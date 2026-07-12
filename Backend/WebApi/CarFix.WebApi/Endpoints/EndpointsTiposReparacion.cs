using CarFix.Aplicacion.Features.TiposReparacion.Commands.ActualizarTipoReparacion;
using CarFix.Aplicacion.Features.TiposReparacion.Commands.CrearTipoReparacion;
using CarFix.Aplicacion.Features.TiposReparacion.Commands.EliminarTipoReparacion;
using CarFix.Aplicacion.Features.TiposReparacion.Dtos;
using CarFix.Aplicacion.Features.TiposReparacion.Queries.ObtenerTiposReparacion;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsTiposReparacion
{
    public static void MapearTiposReparacion(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/tipos-reparacion")
            .WithTags("Catalogo de reparaciones")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<TipoReparacionDto>>>
            (string? filtro, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerTiposReparacionQuery(filtro));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerTiposReparacion")
            .WithSummary("Catalogo de tipos de reparacion con filtro opcional por descripcion");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearTipoReparacionRequest req, ISender sender) =>
            {
                var cmd       = new CrearTipoReparacionCommand(req.DescripcionReparacion, req.DuracionAproximadaHoras, req.CostoBase);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/tipos-reparacion/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("CrearTipoReparacion")
            .WithSummary("Registra un nuevo tipo de reparacion en el catalogo");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarTipoReparacionRequest req, ISender sender) =>
            {
                var cmd      = new ActualizarTipoReparacionCommand(id, req.DescripcionReparacion, req.DuracionAproximadaHoras, req.CostoBase);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("ActualizarTipoReparacion")
            .WithSummary("Actualiza descripcion, duracion y costo base de un tipo de reparacion");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarTipoReparacionCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .RequireAuthorization(p => p.RequireRole("Administrador", "JefeMecanicos"))
            .WithName("EliminarTipoReparacion")
            .WithSummary("Elimina un tipo de reparacion que no este en uso");
    }
}

public record CrearTipoReparacionRequest(
    string  DescripcionReparacion,
    int     DuracionAproximadaHoras,
    decimal CostoBase);

public record ActualizarTipoReparacionRequest(
    string  DescripcionReparacion,
    int     DuracionAproximadaHoras,
    decimal CostoBase);
