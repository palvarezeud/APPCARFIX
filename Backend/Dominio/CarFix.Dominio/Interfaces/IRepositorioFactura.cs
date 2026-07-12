using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioFactura
{
    Task<Factura?>             ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Factura>> ObtenerTodosAsync(CancellationToken ct = default);
    Task                       AgregarAsync(Factura factura, CancellationToken ct = default);
    void                       Actualizar(Factura factura);
    void                       Eliminar(Factura factura);
}
