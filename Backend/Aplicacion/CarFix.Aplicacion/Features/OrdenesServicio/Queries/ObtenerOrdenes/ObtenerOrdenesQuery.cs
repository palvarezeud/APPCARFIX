using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.OrdenesServicio.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Queries.ObtenerOrdenes;

public record ObtenerOrdenesQuery(string? Filtro = null) : IRequest<Resultado<IEnumerable<OrdenServicioDto>>>;
