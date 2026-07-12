using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioCliente : IRepositorioCliente
{
    private readonly CarFixDbContext _contexto;

    public RepositorioCliente(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.Clientes.FindAsync([id], ct);

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.Clientes.OrderBy(c => c.NombreCliente).ToListAsync(ct);

    public async Task AgregarAsync(Cliente cliente, CancellationToken ct = default)
        => await _contexto.Clientes.AddAsync(cliente, ct);

    public void Actualizar(Cliente cliente)
        => _contexto.Clientes.Update(cliente);

    public void Eliminar(Cliente cliente)
        => _contexto.Clientes.Remove(cliente);
}
