using CarFix.Dominio.Excepciones;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarFix.WebApi.Excepciones;

public class ManejadorExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorExcepciones> _logger;

    public ManejadorExcepciones(ILogger<ManejadorExcepciones> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext contextoHttp, Exception excepcion, CancellationToken ct)
    {
        var metodo = contextoHttp.Request.Method;
        var ruta   = contextoHttp.Request.Path;

        var (codigoEstado, titulo, detalle) = excepcion switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Error de validacion",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),

            ExcepcionDominio ex => (
                StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio violada",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrio un error inesperado.")
        };

        switch (codigoEstado)
        {
            case StatusCodes.Status400BadRequest:
                _logger.LogWarning(
                    "400 {Metodo} {Ruta} | {Detalle}",
                    metodo, ruta, detalle);
                break;

            case StatusCodes.Status422UnprocessableEntity:
                _logger.LogWarning(
                    "422 {Metodo} {Ruta} | {Detalle}",
                    metodo, ruta, detalle);
                break;

            default:
                _logger.LogError(
                    excepcion,
                    "500 {Metodo} {Ruta} | Excepcion no controlada",
                    metodo, ruta);
                break;
        }

        contextoHttp.Response.StatusCode = codigoEstado;
        await contextoHttp.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = codigoEstado,
            Title  = titulo,
            Detail = detalle
        }, ct);

        return true;
    }
}
