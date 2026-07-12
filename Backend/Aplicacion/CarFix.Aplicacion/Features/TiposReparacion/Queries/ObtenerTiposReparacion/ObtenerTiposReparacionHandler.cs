using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.TiposReparacion.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.TiposReparacion.Queries.ObtenerTiposReparacion;

public class ObtenerTiposReparacionHandler : IRequestHandler<ObtenerTiposReparacionQuery, Resultado<IEnumerable<TipoReparacionDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerTiposReparacionHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<TipoReparacionDto>>> Handle(ObtenerTiposReparacionQuery query, CancellationToken ct)
    {
        var q = _contexto.TipoReparacions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filtro))
            q = q.Where(t => t.DescripcionReparacion.Contains(query.Filtro));

        var resultado = await q
            .OrderBy(t => t.TipoReparacionId)
            .Select(t => new TipoReparacionDto(
                t.TipoReparacionId,
                t.DescripcionReparacion,
                t.DuracionAproximadaHoras,
                t.CostoBase))
            .ToListAsync(ct);

        return Resultado<IEnumerable<TipoReparacionDto>>.Exito(resultado);
    }
}
