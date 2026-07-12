using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Facturas.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturas;

public record ObtenerFacturasQuery(string? Filtro = null) : IRequest<Resultado<IEnumerable<FacturaDto>>>;
