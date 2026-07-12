namespace CarFix.Aplicacion.Features.OrdenesServicio.Dtos;

public record OrdenServicioDto(
    int      OrdenServicioId,
    int      VehiculoId,
    int      FacturaId,
    string   Placa,
    string   Marca,
    string   Modelo,
    string   NombreCliente,
    DateTime FechaIngreso,
    DateTime FechaSalida,
    string   ProblemaGeneral,
    int      EstadoOrdenId,
    string   EstadoOrdenDescripcion,
    bool     EsGarantia,
    // Datos de la factura asociada (solo lectura)
    DateTime FacturaFecha,
    decimal  FacturaTotalRepuestos,
    decimal  FacturaTotalReparaciones,
    decimal  FacturaDescuento,
    decimal  FacturaAdelanto,
    decimal  FacturaImpuestoVentas,
    decimal  FacturaTotal,
    int      FacturaEstadoId,
    string   FacturaEstadoDescripcion,
    string?  FacturaDescripcionGeneral
);
