using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Commands.EnviarFactura;

public record EnviarFacturaCommand(int FacturaId) : IRequest<Resultado>;
