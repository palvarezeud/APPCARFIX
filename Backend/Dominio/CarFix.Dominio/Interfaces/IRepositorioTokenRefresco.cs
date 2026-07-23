using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IRepositorioTokenRefresco
{
    Task<TokenRefresco?> ObtenerPorHashAsync(string tokenHash, CancellationToken ct = default);
    Task                 AgregarAsync(TokenRefresco token, CancellationToken ct = default);
}
