using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.EliminarVehiculo;

public class EliminarVehiculoHandler : IRequestHandler<EliminarVehiculoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarVehiculoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarVehiculoCommand cmd, CancellationToken ct)
    {
        var vehiculo = await _contexto.Vehiculos
            .Include(v => v.OrdenServicios)
            .FirstOrDefaultAsync(v => v.VehiculoId == cmd.VehiculoId, ct);

        if (vehiculo is null)
            return Resultado.Fallo("Vehiculo no encontrado.");

        if (vehiculo.OrdenServicios.Count > 0)
            return Resultado.Fallo("No se puede eliminar un vehiculo que tiene ordenes de servicio asociadas.");

        _contexto.Vehiculos.Remove(vehiculo);
        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
