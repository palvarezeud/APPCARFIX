using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Clientes.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Clientes.Queries.ObtenerClientes;

public record ObtenerClientesQuery(string? Filtro = null) : IRequest<Resultado<IEnumerable<ClienteDto>>>;
