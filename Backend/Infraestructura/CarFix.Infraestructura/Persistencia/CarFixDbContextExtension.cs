using CarFix.Dominio.Interfaces;

namespace CarFix.Infraestructura.Persistencia;

public partial class CarFixDbContext : ICarFixDbContext
{
    public async Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        => await base.SaveChangesAsync(ct);
}
