namespace CarFix.Aplicacion.Features.MarcasModelos.Dtos;

public record MarcaModeloDto(
    int     MarcaModeloId,
    string? Marca,
    string? Modelo,
    int?    Annio
);
