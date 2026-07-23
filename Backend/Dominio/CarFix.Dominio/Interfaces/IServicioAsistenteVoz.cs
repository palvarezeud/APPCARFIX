namespace CarFix.Dominio.Interfaces;

public interface IServicioAsistenteVoz
{
    Task<InterpretacionVozResultado> InterpretarAsync(
        string transcripcion, string? pantallaActual, string? intentEnProgreso, CancellationToken ct = default);
}

public record ClienteExtraidoVoz(
    string? NombreCliente,
    string? Telefono1,
    string? Telefono2,
    string? Email,
    bool?   EsEmpresa);

public record VehiculoExtraidoVoz(
    string? Placa,
    string? Marca,
    string? Modelo,
    string? Vin,
    int?    Annio,
    string? Motor,
    bool?   EsAutomatico,
    string? NombreClienteBuscado);

public record OrdenExtraidaVoz(
    string? ProblemaGeneral,
    bool?   EsGarantia,
    string? PlacaBuscada);

public record FacturaExtraidaVoz(
    string? NombreClienteBuscado,
    string? PlacaBuscada);

public record ReparacionExtraidaVoz(
    string?  DescripcionReparacion,
    decimal? Costo,
    int?     DuracionAproximadaHoras);

public record RepuestoExtraidoVoz(
    string?  NombreRepuesto,
    decimal? Costo,
    string?  Repuestera,
    string?  NumeroFactura);

public record InterpretacionVozResultado(
    bool    EsExitoso,
    string? MensajeError,
    string? Intent,
    string? PantallaDestino,
    bool    AbrirFormularioCrear,
    string? TerminoBusqueda,
    ClienteExtraidoVoz?    Cliente,
    VehiculoExtraidoVoz?   Vehiculo,
    OrdenExtraidaVoz?      Orden,
    FacturaExtraidaVoz?    Factura,
    ReparacionExtraidaVoz? Reparacion,
    RepuestoExtraidoVoz?   Repuesto,
    string? MensajeParaUsuario)
{
    public static InterpretacionVozResultado Exito(
        string intent, string? pantallaDestino, bool abrirFormularioCrear, string? terminoBusqueda,
        ClienteExtraidoVoz? cliente, VehiculoExtraidoVoz? vehiculo, OrdenExtraidaVoz? orden,
        FacturaExtraidaVoz? factura, ReparacionExtraidaVoz? reparacion, RepuestoExtraidoVoz? repuesto,
        string? mensajeParaUsuario)
        => new(true, null, intent, pantallaDestino, abrirFormularioCrear, terminoBusqueda,
               cliente, vehiculo, orden, factura, reparacion, repuesto, mensajeParaUsuario);

    public static InterpretacionVozResultado Fallo(string mensajeError)
        => new(false, mensajeError, null, null, false, null, null, null, null, null, null, null, null);
}
