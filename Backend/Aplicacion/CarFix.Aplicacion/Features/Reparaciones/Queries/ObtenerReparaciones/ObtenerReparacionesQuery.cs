using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Reparaciones.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Reparaciones.Queries.ObtenerReparaciones;

public record ObtenerReparacionesQuery(int FacturaId) : IRequest<Resultado<IEnumerable<ReparacionDto>>>;
