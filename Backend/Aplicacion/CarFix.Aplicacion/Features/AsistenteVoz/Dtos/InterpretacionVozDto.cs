namespace CarFix.Aplicacion.Features.AsistenteVoz.Dtos;

public record ClienteVozDto(
    string? NombreCliente,
    string? Telefono1,
    string? Telefono2,
    string? Email,
    bool?   EsEmpresa);

public record VehiculoVozDto(
    string? Placa,
    string? Marca,
    string? Modelo,
    string? Vin,
    int?    Annio,
    string? Motor,
    bool?   EsAutomatico,
    string? NombreClienteBuscado);

public record OrdenVozDto(
    string? ProblemaGeneral,
    bool?   EsGarantia,
    string? PlacaBuscada);

public record FacturaVozDto(
    string? NombreClienteBuscado,
    string? PlacaBuscada);

public record ReparacionVozDto(
    string?  DescripcionReparacion,
    decimal? Costo,
    int?     DuracionAproximadaHoras);

public record RepuestoVozDto(
    string?  NombreRepuesto,
    decimal? Costo,
    string?  Repuestera,
    string?  NumeroFactura);

public record InterpretacionVozDto(
    string  Intent,
    string? PantallaDestino,
    bool    AbrirFormularioCrear,
    string? TerminoBusqueda,
    ClienteVozDto?    Cliente,
    VehiculoVozDto?   Vehiculo,
    OrdenVozDto?      Orden,
    FacturaVozDto?    Factura,
    ReparacionVozDto? Reparacion,
    RepuestoVozDto?   Repuesto,
    string? MensajeParaUsuario);
