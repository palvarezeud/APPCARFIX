using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Repuestos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.EscanearFacturaRepuesto;

public class EscanearFacturaRepuestoHandler
    : IRequestHandler<EscanearFacturaRepuestoCommand, Resultado<DatosFacturaRepuestoExtraidosDto>>
{
    private readonly IServicioVisionFacturaRepuesto _servicioVision;

    public EscanearFacturaRepuestoHandler(IServicioVisionFacturaRepuesto servicioVision)
        => _servicioVision = servicioVision;

    public async Task<Resultado<DatosFacturaRepuestoExtraidosDto>> Handle(
        EscanearFacturaRepuestoCommand cmd, CancellationToken ct)
    {
        var resultado = await _servicioVision.ExtraerDatosAsync(cmd.ImagenBytes, cmd.TipoContenido, ct);

        if (!resultado.EsExitoso)
            return Resultado<DatosFacturaRepuestoExtraidosDto>.Fallo(
                resultado.MensajeError ?? "No se pudo procesar la imagen.");

        var nombreConcatenado = resultado.NombresRepuestos is { Count: > 0 }
            ? string.Join(", ", resultado.NombresRepuestos)
            : null;

        var dto = new DatosFacturaRepuestoExtraidosDto(
            nombreConcatenado, resultado.MontoTotal, resultado.Fecha,
            resultado.Repuestera, resultado.NumeroFactura);

        return Resultado<DatosFacturaRepuestoExtraidosDto>.Exito(dto);
    }
}
