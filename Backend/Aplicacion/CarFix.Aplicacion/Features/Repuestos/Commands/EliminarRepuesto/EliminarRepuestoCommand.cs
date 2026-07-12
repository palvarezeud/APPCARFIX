using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.EliminarRepuesto;

public record EliminarRepuestoCommand(int RepuestoId) : IRequest<Resultado>;
