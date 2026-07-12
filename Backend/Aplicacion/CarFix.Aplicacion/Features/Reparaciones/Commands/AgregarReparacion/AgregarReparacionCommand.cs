using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.AgregarReparacion;

public record AgregarReparacionCommand(
    int     FacturaId,
    string  DescripcionReparacion,
    decimal Costo,
    int?    DuracionAproximadaHoras = null
) : IRequest<Resultado<int>>;
