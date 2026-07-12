using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Talleres.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Talleres.Queries.ObtenerTaller;

public record ObtenerTallerQuery() : IRequest<Resultado<TallerDto>>;
