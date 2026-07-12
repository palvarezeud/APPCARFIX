using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Commands.CambiarEstadoFactura;

public record CambiarEstadoFacturaCommand(int FacturaId, int NuevoEstadoId) : IRequest<Resultado>;
