using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Vehiculos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.EscanearTarjetaCirculacion;

public class EscanearTarjetaCirculacionHandler
    : IRequestHandler<EscanearTarjetaCirculacionCommand, Resultado<DatosVehiculoExtraidosDto>>
{
    private readonly IServicioVisionVehiculo _servicioVision;

    public EscanearTarjetaCirculacionHandler(IServicioVisionVehiculo servicioVision)
        => _servicioVision = servicioVision;

    public async Task<Resultado<DatosVehiculoExtraidosDto>> Handle(
        EscanearTarjetaCirculacionCommand cmd, CancellationToken ct)
    {
        var resultado = await _servicioVision.ExtraerDatosAsync(cmd.ImagenBytes, cmd.TipoContenido, ct);

        if (!resultado.EsExitoso)
            return Resultado<DatosVehiculoExtraidosDto>.Fallo(
                resultado.MensajeError ?? "No se pudo procesar la imagen.");

        var dto = new DatosVehiculoExtraidosDto(
            resultado.Marca, resultado.Modelo, resultado.Annio,
            resultado.Vin, resultado.Placa, resultado.Motor);

        return Resultado<DatosVehiculoExtraidosDto>.Exito(dto);
    }
}
