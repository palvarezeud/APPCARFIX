using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Queries.ObtenerHistoricoRepuestos;

public record ObtenerHistoricoRepuestosQuery(
    string? Marca  = null,
    string? Modelo = null
) : IRequest<Resultado<IEnumerable<HistoricoRespuestoDto>>>;
