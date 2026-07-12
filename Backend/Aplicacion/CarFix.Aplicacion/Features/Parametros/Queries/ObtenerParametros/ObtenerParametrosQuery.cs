using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Parametros.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Parametros.Queries.ObtenerParametros;

public record ObtenerParametrosQuery() : IRequest<Resultado<IEnumerable<ParametroDto>>>;
