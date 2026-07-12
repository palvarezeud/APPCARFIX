using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.EliminarReparacion;

public record EliminarReparacionCommand(int ReparacionId) : IRequest<Resultado>;
