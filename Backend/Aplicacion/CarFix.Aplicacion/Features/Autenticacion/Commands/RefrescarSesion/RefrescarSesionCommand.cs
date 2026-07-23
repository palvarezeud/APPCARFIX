using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;
using MediatR;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RefrescarSesion;

public record RefrescarSesionCommand(
    string TokenRefresco
) : IRequest<Resultado<RespuestaTokenDto>>;
