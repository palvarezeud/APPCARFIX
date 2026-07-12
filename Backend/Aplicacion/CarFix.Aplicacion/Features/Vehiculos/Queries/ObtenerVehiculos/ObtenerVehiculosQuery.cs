using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Vehiculos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Vehiculos.Queries.ObtenerVehiculos;

public record ObtenerVehiculosQuery(string? Filtro = null) : IRequest<Resultado<IEnumerable<VehiculoDto>>>;
