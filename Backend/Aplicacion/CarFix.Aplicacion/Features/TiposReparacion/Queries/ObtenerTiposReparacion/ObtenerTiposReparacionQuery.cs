using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.TiposReparacion.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.TiposReparacion.Queries.ObtenerTiposReparacion;

public record ObtenerTiposReparacionQuery(string? Filtro = null) : IRequest<Resultado<IEnumerable<TipoReparacionDto>>>;
