using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CambiarEstadoOrden;

public record CambiarEstadoOrdenCommand(int OrdenServicioId, int NuevoEstadoId) : IRequest<Resultado>;
