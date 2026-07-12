using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.ActualizarTipoReparacion;

public record ActualizarTipoReparacionCommand(
    int     TipoReparacionId,
    string  DescripcionReparacion,
    int     DuracionAproximadaHoras,
    decimal CostoBase
) : IRequest<Resultado>;
