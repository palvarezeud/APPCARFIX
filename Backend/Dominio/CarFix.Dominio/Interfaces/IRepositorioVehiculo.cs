using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioVehiculo
{
    Task<Vehiculo?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Vehiculo>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                        AgregarAsync(Vehiculo vehiculo, CancellationToken ct = default);
    void                        Actualizar(Vehiculo vehiculo);
    void                        Eliminar(Vehiculo vehiculo);
}
