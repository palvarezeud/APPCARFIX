using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.EliminarRepuesto;

public class EliminarRepuestoHandler : IRequestHandler<EliminarRepuestoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarRepuestoCommand cmd, CancellationToken ct)
    {
        var repuesto = await _contexto.Repuestos
            .Include(r => r.FacturaNavigation)
            .FirstOrDefaultAsync(r => r.RepuestoId == cmd.RepuestoId, ct);

        if (repuesto is null)
            return Resultado.Fallo("Repuesto no encontrado.");

        if (repuesto.FacturaNavigation.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede eliminar un repuesto de una factura Pagada.");

        var factura = repuesto.FacturaNavigation;
        factura.TotalRepuestos -= repuesto.Costo;
        var subtotal  = factura.TotalRepuestos + factura.TotalReparaciones - factura.Descuento;
        factura.Total = subtotal + subtotal * factura.ImpuestoVentas / 100m;

        _contexto.Repuestos.Remove(repuesto);
        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
