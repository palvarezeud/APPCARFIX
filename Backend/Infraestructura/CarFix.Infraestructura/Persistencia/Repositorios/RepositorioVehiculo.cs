using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioVehiculo : IRepositorioVehiculo
{
    private readonly CarFixDbContext _contexto;

    public RepositorioVehiculo(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<Vehiculo?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.Vehiculos.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.VehiculoId == id, ct);

    public async Task<IEnumerable<Vehiculo>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.Vehiculos.Include(v => v.Cliente).OrderBy(v => v.Marca).ToListAsync(ct);

    public async Task AgregarAsync(Vehiculo vehiculo, CancellationToken ct = default)
        => await _contexto.Vehiculos.AddAsync(vehiculo, ct);

    public void Actualizar(Vehiculo vehiculo)
        => _contexto.Vehiculos.Update(vehiculo);

    public void Eliminar(Vehiculo vehiculo)
        => _contexto.Vehiculos.Remove(vehiculo);
}
