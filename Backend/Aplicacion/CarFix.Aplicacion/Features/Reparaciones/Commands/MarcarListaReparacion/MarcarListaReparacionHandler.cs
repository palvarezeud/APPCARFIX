using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.MarcarListaReparacion;

public class MarcarListaReparacionHandler : IRequestHandler<MarcarListaReparacionCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public MarcarListaReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(MarcarListaReparacionCommand cmd, CancellationToken ct)
    {
        var reparacion = await _contexto.Reparacions
            .Include(r => r.Factura)
            .FirstOrDefaultAsync(r => r.ReparacionId == cmd.ReparacionId, ct);

        if (reparacion is null)
            return Resultado.Fallo("Reparacion no encontrada.");

        if (reparacion.Factura.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede modificar una reparacion de una factura Pagada.");

        reparacion.Listo = cmd.Listo;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
