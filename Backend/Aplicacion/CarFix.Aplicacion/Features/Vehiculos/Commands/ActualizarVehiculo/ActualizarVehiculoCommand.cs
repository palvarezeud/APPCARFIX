using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.ActualizarVehiculo;

public record ActualizarVehiculoCommand(
    int     VehiculoId,
    int     ClienteId,
    string? Placa,
    string  Marca,
    string  Modelo,
    string? Vin,
    short   Annio,
    string? Motor,
    bool    EsAutomatico,
    string  DetallesCarroceria
) : IRequest<Resultado>;
