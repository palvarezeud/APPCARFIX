namespace CarFix.Aplicacion.Features.Repuestos.Dtos;

public record RepuestoDto(
    int      RepuestoId,
    int      FacturaId,
    bool     Incluido,
    string   NombreRepuesto,
    decimal  Costo,
    DateTime Fecha,
    string   Repuestera,
    string?  Factura
);
