using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly CarFixDbContext _contexto;

    public RepositorioUsuario(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => await _contexto.Usuarios.FindAsync([id], ct);

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(CancellationToken ct = default)
        => await _contexto.Usuarios.Include(u => u.Rol).OrderBy(u => u.NombreUsuario).ToListAsync(ct);

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
        => await _contexto.Usuarios.AddAsync(usuario, ct);

    public void Actualizar(Usuario usuario)
        => _contexto.Usuarios.Update(usuario);

    public void Eliminar(Usuario usuario)
        => _contexto.Usuarios.Remove(usuario);
}
