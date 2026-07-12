using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.ActualizarRepuesto;

public record ActualizarRepuestoCommand(
    int      RepuestoId,
    string   NombreRepuesto,
    decimal  Costo,
    DateTime Fecha,
    string   Repuestera,
    string?  NumeroFactura
) : IRequest<Resultado>;
