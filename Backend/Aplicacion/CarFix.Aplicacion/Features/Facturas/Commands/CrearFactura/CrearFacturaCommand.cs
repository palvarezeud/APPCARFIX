using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Commands.CrearFactura;

public record CrearFacturaCommand(
    int      VehiculoId,
    DateTime Fecha,
    string   DescripcionGeneral,
    decimal  Descuento,
    decimal  Adelanto
) : IRequest<Resultado<int>>;
