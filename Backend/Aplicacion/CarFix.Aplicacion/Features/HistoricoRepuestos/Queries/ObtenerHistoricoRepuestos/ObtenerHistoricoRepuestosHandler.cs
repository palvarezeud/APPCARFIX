using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.HistoricoRepuestos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Queries.ObtenerHistoricoRepuestos;

public class ObtenerHistoricoRepuestosHandler
    : IRequestHandler<ObtenerHistoricoRepuestosQuery, Resultado<IEnumerable<HistoricoRespuestoDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerHistoricoRepuestosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<HistoricoRespuestoDto>>> Handle(
        ObtenerHistoricoRepuestosQuery query, CancellationToken ct)
    {
        var q = _contexto.HistoricoRespuestos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Marca))
            q = q.Where(h => h.Marca.Contains(query.Marca));

        if (!string.IsNullOrWhiteSpace(query.Modelo))
            q = q.Where(h => h.Modelo.Contains(query.Modelo));

        var resultado = await q
            .OrderBy(h => h.RespuestoHistoricoId)
            .Select(h => new HistoricoRespuestoDto(
                h.RespuestoHistoricoId,
                h.Marca,
                h.Modelo,
                h.Annio,
                h.RepuestoDecripcion,
                h.Precio,
                h.Repuestera,
                h.FechaCompra))
            .ToListAsync(ct);

        return Resultado<IEnumerable<HistoricoRespuestoDto>>.Exito(resultado);
    }
}
