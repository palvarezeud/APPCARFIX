using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.ActualizarRepuesto;

public class ActualizarRepuestoHandler : IRequestHandler<ActualizarRepuestoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarRepuestoCommand cmd, CancellationToken ct)
    {
        var repuesto = await _contexto.Repuestos
            .Include(r => r.FacturaNavigation)
            .FirstOrDefaultAsync(r => r.RepuestoId == cmd.RepuestoId, ct);

        if (repuesto is null)
            return Resultado.Fallo("Repuesto no encontrado.");

        if (repuesto.FacturaNavigation.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede modificar un repuesto de una factura Pagada.");

        var diferencia = cmd.Costo - repuesto.Costo;

        repuesto.NombreRepuesto = cmd.NombreRepuesto;
        repuesto.Costo          = cmd.Costo;
        repuesto.Fecha          = cmd.Fecha;
        repuesto.Repuestera     = cmd.Repuestera;
        repuesto.Factura        = cmd.NumeroFactura;

        var factura = repuesto.FacturaNavigation;
        factura.TotalRepuestos += diferencia;
        var subtotal  = factura.TotalRepuestos + factura.TotalReparaciones - factura.Descuento;
        factura.Total = subtotal + subtotal * factura.ImpuestoVentas / 100m;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
