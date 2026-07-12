using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Talleres.Commands.ActualizarTaller;

public record ActualizarTallerCommand(
    string   Nombre,
    string   UbicacionDescripcion,
    string   Telefonos,
    string   Email,
    decimal? Latitud,
    decimal? Longitud
) : IRequest<Resultado>;
