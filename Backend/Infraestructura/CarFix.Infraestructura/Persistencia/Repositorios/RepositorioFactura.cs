using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioFactura : IRepositorioFactura
{
    private readonly CarFixDbContext _contexto;

    public RepositorioFactura(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<Factura?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.Facturas
            .Include(f => f.EstadoFactura)
            .Include(f => f.Vehiculo).ThenInclude(v => v.Cliente)
            .Include(f => f.Reparaciones)
            .Include(f => f.Repuestos)
            .FirstOrDefaultAsync(f => f.FacturaId == id, ct);

    public async Task<IEnumerable<Factura>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.Facturas
            .Include(f => f.EstadoFactura)
            .Include(f => f.Vehiculo)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync(ct);

    public async Task AgregarAsync(Factura factura, CancellationToken ct = default)
        => await _contexto.Facturas.AddAsync(factura, ct);

    public void Actualizar(Factura factura)
        => _contexto.Facturas.Update(factura);

    public void Eliminar(Factura factura)
        => _contexto.Facturas.Remove(factura);
}
