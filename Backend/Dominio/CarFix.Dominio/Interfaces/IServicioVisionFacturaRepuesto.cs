namespace CarFix.Dominio.Interfaces;

public interface IServicioVisionFacturaRepuesto
{
    Task<ExtraccionFacturaRepuestoResultado> ExtraerDatosAsync(
        byte[] imagenBytes, string tipoContenido, CancellationToken ct = default);
}

public record ExtraccionFacturaRepuestoResultado(
    bool          EsExitoso,
    string?       MensajeError,
    List<string>? NombresRepuestos,
    decimal?      MontoTotal,
    string?       Fecha,
    string?       Repuestera,
    string?       NumeroFactura)
{
    public static ExtraccionFacturaRepuestoResultado Exito(
        List<string>? nombresRepuestos, decimal? montoTotal, string? fecha, string? repuestera, string? numeroFactura)
        => new(true, null, nombresRepuestos, montoTotal, fecha, repuestera, numeroFactura);

    public static ExtraccionFacturaRepuestoResultado Fallo(string mensajeError)
        => new(false, mensajeError, null, null, null, null, null);
}
