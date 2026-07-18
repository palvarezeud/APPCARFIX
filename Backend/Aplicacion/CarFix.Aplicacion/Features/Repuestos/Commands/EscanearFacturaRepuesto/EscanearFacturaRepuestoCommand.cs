using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Repuestos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.EscanearFacturaRepuesto;

public record EscanearFacturaRepuestoCommand(
    byte[] ImagenBytes,
    string TipoContenido
) : IRequest<Resultado<DatosFacturaRepuestoExtraidosDto>>;
