using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioOrdenServicio : IRepositorioOrdenServicio
{
    private readonly CarFixDbContext _contexto;

    public RepositorioOrdenServicio(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<OrdenServicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.OrdenServicios
            .Include(o => o.EstadoOrden)
            .Include(o => o.Vehiculo)
            .Include(o => o.Factura)
            .FirstOrDefaultAsync(o => o.OrdenServicioId == id, ct);

    public async Task<IEnumerable<OrdenServicio>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.OrdenServicios
            .Include(o => o.EstadoOrden)
            .Include(o => o.Vehiculo)
            .OrderByDescending(o => o.FechaIngreso)
            .ToListAsync(ct);

    public async Task AgregarAsync(OrdenServicio orden, CancellationToken ct = default)
        => await _contexto.OrdenServicios.AddAsync(orden, ct);

    public void Actualizar(OrdenServicio orden)
        => _contexto.OrdenServicios.Update(orden);

    public void Eliminar(OrdenServicio orden)
        => _contexto.OrdenServicios.Remove(orden);
}
