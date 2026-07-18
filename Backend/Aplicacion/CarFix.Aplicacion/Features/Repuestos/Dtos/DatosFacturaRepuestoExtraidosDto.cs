namespace CarFix.Aplicacion.Features.Repuestos.Dtos;

public record DatosFacturaRepuestoExtraidosDto(
    string?  NombreRepuesto,
    decimal? Costo,
    string?  Fecha,
    string?  Repuestera,
    string?  NumeroFactura
);
