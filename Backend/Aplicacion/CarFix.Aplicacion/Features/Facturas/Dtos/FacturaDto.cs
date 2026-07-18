namespace CarFix.Aplicacion.Features.Facturas.Dtos;

public record FacturaDto(
    int      FacturaId,
    int      VehiculoId,
    string   Placa,
    string   Marca,
    string   Modelo,
    DateTime Fecha,
    string   NombreCliente,
    string?  EmailCliente,
    string?  DescripcionGeneral,
    decimal  TotalRepuestos,
    decimal  TotalReparaciones,
    decimal  Descuento,
    decimal  SubTotal,
    decimal  ImpuestoVentas,
    decimal  Total,
    decimal  Adelanto,
    decimal  Pendiente,
    int      EstadoFacturaId,
    string   EstadoFacturaDescripcion
);
