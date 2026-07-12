using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Commands.EliminarFactura;

public record EliminarFacturaCommand(int FacturaId) : IRequest<Resultado>;
