namespace CarFix.Dominio.Interfaces;

public interface IServicioVisionVehiculo
{
    Task<ExtraccionVehiculoResultado> ExtraerDatosAsync(
        byte[] imagenBytes, string tipoContenido, CancellationToken ct = default);
}

public record ExtraccionVehiculoResultado(
    bool    EsExitoso,
    string? MensajeError,
    string? Marca,
    string? Modelo,
    short?  Annio,
    string? Vin,
    string? Placa,
    string? Motor)
{
    public static ExtraccionVehiculoResultado Exito(
        string? marca, string? modelo, short? annio, string? vin, string? placa, string? motor)
        => new(true, null, marca, modelo, annio, vin, placa, motor);

    public static ExtraccionVehiculoResultado Fallo(string mensajeError)
        => new(false, mensajeError, null, null, null, null, null, null);
}
