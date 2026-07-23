using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Infraestructura.Persistencia.Repositorios;

public class RepositorioTokenRefresco : IRepositorioTokenRefresco
{
    private readonly CarFixDbContext _contexto;

    public RepositorioTokenRefresco(CarFixDbContext contexto) => _contexto = contexto;

    public async Task<TokenRefresco?> ObtenerPorHashAsync(string tokenHash, CancellationToken ct = default)
        => await _contexto.TokenRefrescos
            .Include(t => t.Usuario).ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AgregarAsync(TokenRefresco token, CancellationToken ct = default)
        => await _contexto.TokenRefrescos.AddAsync(token, ct);
}
