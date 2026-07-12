using CarFix.Dominio.Interfaces;

namespace CarFix.Infraestructura.Persistencia;

public class UnidadTrabajo : IUnidadTrabajo
{
    private readonly CarFixDbContext _contexto;

    public UnidadTrabajo(CarFixDbContext contexto) => _contexto = contexto;

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        => _contexto.SaveChangesAsync(ct);
}
