using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.OrdenesServicio.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CrearOrden;

public record CrearOrdenCommand(
    int      VehiculoId,
    DateTime FechaIngreso,
    DateTime FechaSalida,
    string   ProblemaGeneral,
    bool     EsGarantia
) : IRequest<Resultado<CrearOrdenResponseDto>>;
