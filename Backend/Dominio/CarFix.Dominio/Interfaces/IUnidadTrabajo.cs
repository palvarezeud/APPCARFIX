namespace CarFix.Dominio.Interfaces;

public interface IUnidadTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
