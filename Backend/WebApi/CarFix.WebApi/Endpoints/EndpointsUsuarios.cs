using System.Security.Claims;
using CarFix.Aplicacion.Features.Usuarios.Commands.ActualizarUsuario;
using CarFix.Aplicacion.Features.Usuarios.Commands.CambiarContrasenna;
using CarFix.Aplicacion.Features.Usuarios.Commands.CrearUsuario;
using CarFix.Aplicacion.Features.Usuarios.Commands.EliminarUsuario;
using CarFix.Aplicacion.Features.Usuarios.Dtos;
using CarFix.Aplicacion.Features.Usuarios.Queries.ObtenerUsuarios;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarFix.WebApi.Endpoints;

public static class EndpointsUsuarios
{
    public static void MapearUsuarios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(policy => policy.RequireRole("Administrador", "JefeMecanicos"));

        grupo.MapGet("/",
            async Task<Ok<IEnumerable<UsuarioDto>>>
            (ISender sender) =>
            {
                var resultado = await sender.Send(new ObtenerUsuariosQuery());
                return TypedResults.Ok(resultado.Valor);
            })
            .WithName("ObtenerUsuarios")
            .WithSummary("Lista todos los usuarios del sistema");

        grupo.MapPost("/",
            async Task<Results<Created<int>, BadRequest<string>>>
            (CrearUsuarioCommand cmd, ISender sender) =>
            {
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.Created($"/api/usuarios/{resultado.Valor}", resultado.Valor)
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CrearUsuario")
            .WithSummary("Registra un nuevo usuario en el sistema");

        grupo.MapPut("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, ActualizarUsuarioRequest req, ISender sender) =>
            {
                var cmd = new ActualizarUsuarioCommand(id, req.NombreCompleto, req.Email, req.RolId, req.Activo);
                var resultado = await sender.Send(cmd);
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("ActualizarUsuario")
            .WithSummary("Actualiza nombre, email, rol y estado activo de un usuario");

        grupo.MapPatch("/{id:int}/contrasenna",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, CambiarContrasennaRequest req, ISender sender) =>
            {
                var resultado = await sender.Send(new CambiarContrasennaCommand(id, req.NuevoPassword));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("CambiarContrasenna")
            .WithSummary("Restablece la contrasenna de un usuario");

        grupo.MapDelete("/{id:int}",
            async Task<Results<NoContent, BadRequest<string>>>
            (int id, HttpContext httpContext, ISender sender) =>
            {
                var uidClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var usuarioActualId = int.TryParse(uidClaim, out var uid) ? uid : 0;
                var resultado = await sender.Send(new EliminarUsuarioCommand(id, usuarioActualId));
                return resultado.EsExitoso
                    ? TypedResults.NoContent()
                    : TypedResults.BadRequest(resultado.Error);
            })
            .WithName("EliminarUsuario")
            .WithSummary("Elimina un usuario (no es posible eliminar el propio usuario)");
    }
}

public record ActualizarUsuarioRequest(string NombreCompleto, string? Email, int RolId, bool Activo);
public record CambiarContrasennaRequest(string NuevoPassword);
