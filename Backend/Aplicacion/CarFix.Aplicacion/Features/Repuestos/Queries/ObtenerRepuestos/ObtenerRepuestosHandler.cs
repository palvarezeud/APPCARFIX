using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Repuestos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Repuestos.Queries.ObtenerRepuestos;

public class ObtenerRepuestosHandler : IRequestHandler<ObtenerRepuestosQuery, Resultado<IEnumerable<RepuestoDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerRepuestosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<RepuestoDto>>> Handle(ObtenerRepuestosQuery query, CancellationToken ct)
    {
        var resultado = await _contexto.Repuestos
            .Where(r => r.FacturaId == query.FacturaId)
            .Select(r => new RepuestoDto(
                r.RepuestoId,
                r.FacturaId,
                r.Incluido,
                r.NombreRepuesto,
                r.Costo,
                r.Fecha,
                r.Repuestera,
                r.Factura))
            .ToListAsync(ct);

        return Resultado<IEnumerable<RepuestoDto>>.Exito(resultado);
    }
}
