namespace CarFix.Aplicacion.Features.TiposReparacion.Dtos;

public record TipoReparacionDto(
    int     TipoReparacionId,
    string  DescripcionReparacion,
    int     DuracionAproximadaHoras,
    decimal CostoBase
);
