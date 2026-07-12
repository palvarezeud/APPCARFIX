using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.ActualizarUsuario;

public record ActualizarUsuarioCommand(
    int     UsuarioId,
    string  NombreCompleto,
    string? Email,
    int     RolId,
    bool    Activo
) : IRequest<Resultado>;
