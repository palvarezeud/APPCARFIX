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

public record InterpretacionVozDto(
    string  Intent,
    string? PantallaDestino,
    bool    AbrirFormularioCrear,
    string? TerminoBusqueda,
    ClienteVozDto?  Cliente,
    VehiculoVozDto? Vehiculo,
    OrdenVozDto?    Orden,
    string? MensajeParaUsuario);
