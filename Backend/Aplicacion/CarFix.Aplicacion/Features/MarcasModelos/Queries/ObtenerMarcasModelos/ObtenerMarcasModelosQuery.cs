using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.MarcasModelos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.MarcasModelos.Queries.ObtenerMarcasModelos;

public record ObtenerMarcasModelosQuery : IRequest<Resultado<IEnumerable<MarcaModeloDto>>>;
