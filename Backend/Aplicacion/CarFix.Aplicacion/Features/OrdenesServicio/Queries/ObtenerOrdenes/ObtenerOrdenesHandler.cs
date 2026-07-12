using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.OrdenesServicio.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Queries.ObtenerOrdenes;

public class ObtenerOrdenesHandler : IRequestHandler<ObtenerOrdenesQuery, Resultado<IEnumerable<OrdenServicioDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerOrdenesHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<OrdenServicioDto>>> Handle(ObtenerOrdenesQuery query, CancellationToken ct)
    {
        var q = _contexto.OrdenServicios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filtro))
            q = q.Where(o => o.OrdenServicioId.ToString().Contains(query.Filtro) ||
                              o.Vehiculo.Placa!.Contains(query.Filtro));

        var resultado = await q
            .Include(o => o.EstadoOrden)
            .Include(o => o.Vehiculo).ThenInclude(v => v.Cliente)
            .Include(o => o.Factura).ThenInclude(f => f.EstadoFactura)
            .OrderByDescending(o => o.OrdenServicioId)
            .Select(o => new OrdenServicioDto(
                o.OrdenServicioId,
                o.VehiculoId,
                o.FacturaId,
                o.Vehiculo.Placa ?? "—",
                o.Vehiculo.Marca,
                o.Vehiculo.Modelo ?? "—",
                o.Vehiculo.Cliente.NombreCliente,
                o.FechaIngreso,
                o.FechaSalida,
                o.ProblemaGeneral,
                o.EstadoOrdenId,
                o.EstadoOrden.Descripcion,
                o.EsGarantia,
                o.Factura.Fecha,
                o.Factura.TotalRepuestos,
                o.Factura.TotalReparaciones,
                o.Factura.Descuento,
                o.Factura.Adelanto,
                o.Factura.ImpuestoVentas,
                o.Factura.Total,
                o.Factura.EstadoFacturaId,
                o.Factura.EstadoFactura.Descipcion.Trim(),
                o.Factura.DescripcionGeneral))
            .ToListAsync(ct);

        return Resultado<IEnumerable<OrdenServicioDto>>.Exito(resultado);
    }
}
