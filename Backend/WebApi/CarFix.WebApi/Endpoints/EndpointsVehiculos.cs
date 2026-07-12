using CarFix.Aplicacion.Features.Vehiculos.Commands.ActualizarVehiculo;
using CarFix.Aplicacion.Features.Vehiculos.Commands.CrearVehiculo;
using CarFix.Aplicacion.Features.Vehiculos.Commands.EliminarVehiculo;
using CarFix.Aplicacion.Features.Vehiculos.Commands.EscanearTarjetaCirculacion;
using CarFix.Aplicacion.Features.Vehiculos.Dtos;
using CarFix.Aplicacion.Features.Vehiculos.Queries.ObtenerVehiculos;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsVehiculos
{
    public static void MapearVehiculos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/vehiculos")
            .WithTags("Vehiculos")
            .RequireAuthorization();

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<VehiculoDto>>>
            (string? filtro, ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerVehiculosQuery(filtro));
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerVehiculos")
            .WithSummary("Lista de vehiculos con filtro opcional por placa o nombre del cliente");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearVehiculoCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/vehiculos/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearVehiculo")
            .WithSummary("Registra un nuevo vehiculo");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarVehiculoRequest req, ISender sender) =>
            {
                var cmd = new ActualizarVehiculoCommand(
                    id, req.ClienteId, req.Placa, req.Marca, req.Modelo,
                    req.Vin, req.Annio, req.Motor, req.EsAutomatico, req.DetallesCarroceria);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarVehiculo")
            .WithSummary("Actualiza un vehiculo existente");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ISender sender) =>
            {
                var resultado = await sender.Send(new EliminarVehiculoCommand(id));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarVehiculo")
            .WithSummary("Elimina un vehiculo sin ordenes de servicio asociadas");

        grupo.MapPost("/escanear-tarjeta-circulacion",
            async Task<Results<Ok<DatosVehiculoExtraidosDto>, BadRequest<string>>>
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

                var cmd       = new EscanearTarjetaCirculacionCommand(memoria.ToArray(), foto.ContentType);
                var resultado = await sender.Send(cmd);

                return resultado.EsExitoso
                    ? TypedResults.Ok(resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EscanearTarjetaCirculacion")
            .WithSummary("Extrae datos del vehiculo desde una foto de la tarjeta de circulacion usando IA")
            .DisableAntiforgery();
    }
}

public record ActualizarVehiculoRequest(
    int     ClienteId,
    string? Placa,
    string  Marca,
    string  Modelo,
    string? Vin,
    short   Annio,
    string? Motor,
    bool    EsAutomatico,
    string  DetallesCarroceria);
