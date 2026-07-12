using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioOrdenServicio
{
    Task<OrdenServicio?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<OrdenServicio>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                             AgregarAsync(OrdenServicio orden, CancellationToken ct = default);
    void                             Actualizar(OrdenServicio orden);
    void                             Eliminar(OrdenServicio orden);
}
