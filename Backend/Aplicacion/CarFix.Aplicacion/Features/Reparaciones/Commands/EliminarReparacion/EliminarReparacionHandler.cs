using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.EliminarReparacion;

public class EliminarReparacionHandler : IRequestHandler<EliminarReparacionCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarReparacionCommand cmd, CancellationToken ct)
    {
        var reparacion = await _contexto.Reparacions
            .Include(r => r.Factura)
            .FirstOrDefaultAsync(r => r.ReparacionId == cmd.ReparacionId, ct);

        if (reparacion is null)
            return Resultado.Fallo("Reparacion no encontrada.");

        if (reparacion.Factura.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede eliminar una reparacion de una factura Pagada.");

        var factura = reparacion.Factura;
        factura.TotalReparaciones -= reparacion.Costo;

        await RecalculadorTotalesFactura.RecalcularAsync(_contexto, factura, ct);

        _contexto.Reparacions.Remove(reparacion);

        await RecalculadorFechaSalidaFactura.RecalcularFechaSalidaAsync(
            _contexto, factura.FacturaId, null, reparacion.ReparacionId, ct);

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
