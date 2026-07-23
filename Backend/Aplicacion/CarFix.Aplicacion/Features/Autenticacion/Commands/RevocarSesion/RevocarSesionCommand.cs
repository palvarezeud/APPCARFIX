using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RevocarSesion;

public record RevocarSesionCommand(
    string TokenRefresco
) : IRequest<Resultado>;
