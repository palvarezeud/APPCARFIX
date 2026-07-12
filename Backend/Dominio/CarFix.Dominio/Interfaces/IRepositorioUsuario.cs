using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioUsuario
{
    Task<Usuario?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                       AgregarAsync(Usuario usuario, CancellationToken ct = default);
    void                       Actualizar(Usuario usuario);
    void                       Eliminar(Usuario usuario);
}
