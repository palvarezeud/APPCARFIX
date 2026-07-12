using CarFix.Aplicacion.Features.Parametros.Commands.ActualizarParametro;
using CarFix.Aplicacion.Features.Parametros.Commands.CrearParametro;
using CarFix.Aplicacion.Features.Parametros.Commands.EliminarParametro;
using CarFix.Aplicacion.Features.Parametros.Dtos;
using CarFix.Aplicacion.Features.Parametros.Queries.ObtenerParametros;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsParametros
{
    public static void MapearParametros(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/parametros")
            .WithTags("Parametros")
            .RequireAuthorization(policy => policy.RequireRole("Administrador"));

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<ParametroDto>>>
            (ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerParametrosQuery());
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerParametros")
            .WithSummary("Lista los parametros de configuracion del sistema");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearParametroCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/parametros/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearParametro")
            .WithSummary("Registra un nuevo parametro de configuracion");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarParametroRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new ActualizarParametroCommand(id, req.Nombre, req.Valor));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarParametro")
            .WithSummary("Actualiza un parametro de configuracion existente");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarParametroCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarParametro")
            .WithSummary("Elimina un parametro de configuracion");
    }
}

file record ActualizarParametroRequest(string Nombre, string Valor);
