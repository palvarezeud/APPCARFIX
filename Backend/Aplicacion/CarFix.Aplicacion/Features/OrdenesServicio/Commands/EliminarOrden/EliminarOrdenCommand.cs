using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.EliminarOrden;

public record EliminarOrdenCommand(int OrdenServicioId) : IRequest<Resultado>;
