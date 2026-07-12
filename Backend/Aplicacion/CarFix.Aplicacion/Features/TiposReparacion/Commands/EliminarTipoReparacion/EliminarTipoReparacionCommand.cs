using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.EliminarTipoReparacion;

public record EliminarTipoReparacionCommand(int TipoReparacionId) : IRequest<Resultado>;
