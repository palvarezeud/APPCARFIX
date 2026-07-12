using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Facturas.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturas;

public class ObtenerFacturasHandler : IRequestHandler<ObtenerFacturasQuery, Resultado<IEnumerable<FacturaDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerFacturasHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<FacturaDto>>> Handle(ObtenerFacturasQuery query, CancellationToken ct)
    {
        var q = _contexto.Facturas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filtro))
            q = q.Where(f =>
                f.NombreCliente.Contains(query.Filtro) ||
                (f.Vehiculo.Placa != null && f.Vehiculo.Placa.Contains(query.Filtro)) ||
                f.FacturaId.ToString().Contains(query.Filtro));

        var resultado = await q
            .Include(f => f.EstadoFactura)
            .Include(f => f.Vehiculo)
            .OrderByDescending(f => f.FacturaId)
            .Select(f => new FacturaDto(
                f.FacturaId,
                f.VehiculoId,
                f.Vehiculo.Placa ?? "—",
                f.Vehiculo.Marca,
                f.Vehiculo.Modelo ?? "—",
                f.Fecha,
                f.NombreCliente,
                f.Vehiculo.Cliente.Email,
                f.DescripcionGeneral,
                f.TotalRepuestos,
                f.TotalReparaciones,
                f.Total,
                f.Descuento,
                f.Adelanto,
                f.ImpuestoVentas,
                f.EstadoFacturaId,
                f.EstadoFactura.Descipcion.Trim()))
            .ToListAsync(ct);

        return Resultado<IEnumerable<FacturaDto>>.Exito(resultado);
    }
}
