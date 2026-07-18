using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Commands.ActualizarFactura;

public record ActualizarFacturaCommand(
    int      FacturaId,
    DateTime Fecha,
    string?  DescripcionGeneral,
    decimal  Descuento,
    decimal  Adelanto
) : IRequest<Resultado>;
