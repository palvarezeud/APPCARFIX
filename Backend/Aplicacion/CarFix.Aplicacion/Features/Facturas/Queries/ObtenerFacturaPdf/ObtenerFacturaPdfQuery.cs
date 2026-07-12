using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturaPdf;

public record ObtenerFacturaPdfQuery(int FacturaId) : IRequest<Resultado<byte[]>>;
