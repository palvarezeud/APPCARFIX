using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.CrearTipoReparacion;

public record CrearTipoReparacionCommand(
    string  DescripcionReparacion,
    int     DuracionAproximadaHoras,
    decimal CostoBase
) : IRequest<Resultado<int>>;
