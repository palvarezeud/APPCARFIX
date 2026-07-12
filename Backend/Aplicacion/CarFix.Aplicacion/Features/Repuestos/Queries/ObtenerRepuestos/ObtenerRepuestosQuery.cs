using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Repuestos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Queries.ObtenerRepuestos;

public record ObtenerRepuestosQuery(int FacturaId) : IRequest<Resultado<IEnumerable<RepuestoDto>>>;
