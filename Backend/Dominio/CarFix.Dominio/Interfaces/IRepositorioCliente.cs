using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioCliente
{
    Task<Cliente?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                       AgregarAsync(Cliente cliente, CancellationToken ct = default);
    void                       Actualizar(Cliente cliente);
    void                       Eliminar(Cliente cliente);
}
