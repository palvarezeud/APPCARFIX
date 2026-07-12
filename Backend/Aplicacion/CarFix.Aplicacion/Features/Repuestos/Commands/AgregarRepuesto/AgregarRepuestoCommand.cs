using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.AgregarRepuesto;

public record AgregarRepuestoCommand(
    int      FacturaId,
    string   NombreRepuesto,
    decimal  Costo,
    DateTime Fecha,
    string   Repuestera,
    string?  NumeroFactura
) : IRequest<Resultado<int>>;
