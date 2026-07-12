namespace CarFix.Aplicacion.Features.Reparaciones.Dtos;

public record ReparacionDto(
    int      ReparacionId,
    int      FacturaId,
    bool     Listo,
    string   DescripcionReparacion,
    int?     DuracionAproximadaHoras,
    decimal  Costo
);
