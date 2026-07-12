using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.ActualizarOrden;

public record ActualizarOrdenCommand(
    int      OrdenServicioId,
    int      VehiculoId,
    DateTime FechaIngreso,
    DateTime FechaSalida,
    string   ProblemaGeneral,
    bool     EsGarantia,
    int      EstadoOrdenId
) : IRequest<Resultado>;
