using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Vehiculos.Dtos;
using MediatR;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.EscanearTarjetaCirculacion;

public record EscanearTarjetaCirculacionCommand(
    byte[] ImagenBytes,
    string TipoContenido
) : IRequest<Resultado<DatosVehiculoExtraidosDto>>;
