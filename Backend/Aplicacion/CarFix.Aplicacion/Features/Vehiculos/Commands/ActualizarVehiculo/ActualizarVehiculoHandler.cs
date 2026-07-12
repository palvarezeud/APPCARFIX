using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.ActualizarVehiculo;

public class ActualizarVehiculoHandler : IRequestHandler<ActualizarVehiculoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarVehiculoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarVehiculoCommand cmd, CancellationToken ct)
    {
        var vehiculo = await _contexto.Vehiculos
            .FirstOrDefaultAsync(v => v.VehiculoId == cmd.VehiculoId, ct);

        if (vehiculo is null)
            return Resultado.Fallo("Vehiculo no encontrado.");

        var clienteExiste = await _contexto.Clientes.AnyAsync(c => c.ClienteId == cmd.ClienteId, ct);
        if (!clienteExiste)
            return Resultado.Fallo("El cliente no existe.");

        vehiculo.ClienteId          = cmd.ClienteId;
        vehiculo.Placa              = string.IsNullOrWhiteSpace(cmd.Placa) ? null : cmd.Placa.Trim();
        vehiculo.Marca              = cmd.Marca.Trim();
        vehiculo.Modelo             = cmd.Modelo.Trim();
        vehiculo.Vin                = string.IsNullOrWhiteSpace(cmd.Vin) ? null : cmd.Vin.Trim();
        vehiculo.Annio              = cmd.Annio;
        vehiculo.Motor              = string.IsNullOrWhiteSpace(cmd.Motor) ? null : cmd.Motor.Trim();
        vehiculo.EsAutomatico       = cmd.EsAutomatico;
        vehiculo.DetallesCarroceria = cmd.DetallesCarroceria.Trim();

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
