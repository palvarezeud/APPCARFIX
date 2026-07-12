namespace CarFix.Aplicacion.Features.Vehiculos.Dtos;

public record DatosVehiculoExtraidosDto(
    string? Marca,
    string? Modelo,
    short?  Annio,
    string? Vin,
    string? Placa,
    string? Motor);
