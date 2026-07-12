using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Reparaciones.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Reparaciones.Queries.ObtenerReparaciones;

public class ObtenerReparacionesHandler : IRequestHandler<ObtenerReparacionesQuery, Resultado<IEnumerable<ReparacionDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerReparacionesHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<ReparacionDto>>> Handle(ObtenerReparacionesQuery query, CancellationToken ct)
    {
        var resultado = await _contexto.Reparacions
            .Where(r => r.FacturaId == query.FacturaId)
            .Select(r => new ReparacionDto(
                r.ReparacionId,
                r.FacturaId,
                r.Listo,
                r.DescripcionReparacion,
                r.DuracionAproximadaHoras,
                r.Costo))
            .ToListAsync(ct);

        return Resultado<IEnumerable<ReparacionDto>>.Exito(resultado);
    }
}
