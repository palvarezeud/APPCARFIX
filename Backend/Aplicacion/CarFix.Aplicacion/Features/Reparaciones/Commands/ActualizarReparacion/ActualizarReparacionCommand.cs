using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.ActualizarReparacion;

public record ActualizarReparacionCommand(
    int     ReparacionId,
    string  DescripcionReparacion,
    decimal Costo,
    int?    DuracionAproximadaHoras = null
) : IRequest<Resultado>;
