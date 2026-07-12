using CarFix.Aplicacion.Features.Clientes.Commands.ActualizarCliente;
using CarFix.Aplicacion.Features.Clientes.Commands.CrearCliente;
using CarFix.Aplicacion.Features.Clientes.Commands.EliminarCliente;
using CarFix.Aplicacion.Features.Clientes.Dtos;
using CarFix.Aplicacion.Features.Clientes.Queries.ObtenerClientes;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsClientes
{
    public static void MapearClientes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/clientes")
            .WithTags("Clientes")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<ClienteDto>>>
            (string? filtro, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerClientesQuery(filtro));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerClientes")
            .WithSummary("Lista de clientes con filtro opcional por nombre");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearClienteCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/clientes/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearCliente")
            .WithSummary("Registra un nuevo cliente");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>, NotFound<string>>>
            (int id, ActualizarClienteRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new ActualizarClienteCommand(
                    id, req.NombreCliente, req.Telefono1, req.Telefono2, req.Email, req.EsEmpresa));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarCliente")
            .WithSummary("Actualiza los datos de un cliente existente");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarClienteCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarCliente")
            .WithSummary("Elimina un cliente que no tenga facturas asociadas");
    }
}

file record ActualizarClienteRequest(
    string  NombreCliente,
    string  Telefono1,
    string? Telefono2,
    string? Email,
    bool    EsEmpresa);
