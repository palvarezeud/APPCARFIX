using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.CrearVehiculo;

public class CrearVehiculoHandler : IRequestHandler<CrearVehiculoCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearVehiculoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearVehiculoCommand cmd, CancellationToken ct)
    {
        var clienteExiste = await _contexto.Clientes.AnyAsync(c => c.ClienteId == cmd.ClienteId, ct);
        if (!clienteExiste)
            return Resultado<int>.Fallo("El cliente no existe.");

        var vehiculo = new Vehiculo
        {
            ClienteId          = cmd.ClienteId,
            Placa              = string.IsNullOrWhiteSpace(cmd.Placa) ? null : cmd.Placa.Trim(),
            Marca              = cmd.Marca.Trim(),
            Modelo             = cmd.Modelo.Trim(),
            Vin                = string.IsNullOrWhiteSpace(cmd.Vin) ? null : cmd.Vin.Trim(),
            Annio              = cmd.Annio,
            Motor              = string.IsNullOrWhiteSpace(cmd.Motor) ? null : cmd.Motor.Trim(),
            EsAutomatico       = cmd.EsAutomatico,
            DetallesCarroceria = cmd.DetallesCarroceria.Trim()
        };

        await _contexto.Vehiculos.AddAsync(vehiculo, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(vehiculo.VehiculoId);
    }
}
