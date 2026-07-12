using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;

public record IniciarSesionCommand(
    string NombreUsuario,
    string Password
) : IRequest<Resultado<RespuestaTokenDto>>;
