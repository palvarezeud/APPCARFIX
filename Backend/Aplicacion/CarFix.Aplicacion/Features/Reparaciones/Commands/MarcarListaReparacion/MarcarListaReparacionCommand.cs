using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.MarcarListaReparacion;

public record MarcarListaReparacionCommand(
    int  ReparacionId,
    bool Listo
) : IRequest<Resultado>;
