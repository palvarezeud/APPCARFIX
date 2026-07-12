namespace CarFix.Aplicacion.Features.Vehiculos.Dtos;

public record VehiculoDto(
    int     VehiculoId,
    int     ClienteId,
    string  NombreCliente,
    string? Placa,
    string  Marca,
    string? Modelo,
    string? Vin,
    short?  Annio,
    string? Motor,
    bool    EsAutomatico,
    string  DetallesCarroceria
);
