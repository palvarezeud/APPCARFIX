using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.EliminarVehiculo;

public record EliminarVehiculoCommand(int VehiculoId) : IRequest<Resultado>;
