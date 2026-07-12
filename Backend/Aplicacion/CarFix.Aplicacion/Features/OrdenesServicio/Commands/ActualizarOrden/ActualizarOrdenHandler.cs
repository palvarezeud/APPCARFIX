using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.ActualizarOrden;

public class ActualizarOrdenHandler : IRequestHandler<ActualizarOrdenCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarOrdenHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarOrdenCommand cmd, CancellationToken ct)
    {
        var orden = await _contexto.OrdenServicios
            .FirstOrDefaultAsync(o => o.OrdenServicioId == cmd.OrdenServicioId, ct);

        if (orden is null)
            return Resultado.Fallo("Orden no encontrada.");

        if (orden.EstadoOrdenId is 4 or 5)
            return Resultado.Fallo("No se puede modificar una orden en estado Finalizado o Entregado.");

        var vehiculoExiste = await _contexto.Vehiculos.AnyAsync(v => v.VehiculoId == cmd.VehiculoId, ct);
        if (!vehiculoExiste)
            return Resultado.Fallo("El vehiculo no existe.");

        orden.VehiculoId      = cmd.VehiculoId;
        orden.FechaIngreso    = cmd.FechaIngreso;
        orden.FechaSalida     = cmd.FechaSalida;
        orden.ProblemaGeneral = cmd.ProblemaGeneral.Trim();
        orden.EsGarantia      = cmd.EsGarantia;
        orden.EstadoOrdenId   = cmd.EstadoOrdenId;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
