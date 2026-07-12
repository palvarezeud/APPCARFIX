using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CrearUsuario;

public record CrearUsuarioCommand(
    string  NombreUsuario,
    string  Password,
    string  NombreCompleto,
    string? Email,
    int     RolId,
    bool    Activo
) : IRequest<Resultado<int>>;
