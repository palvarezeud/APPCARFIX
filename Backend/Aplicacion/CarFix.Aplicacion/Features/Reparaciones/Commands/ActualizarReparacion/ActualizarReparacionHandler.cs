using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.ActualizarReparacion;

public class ActualizarReparacionHandler : IRequestHandler<ActualizarReparacionCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarReparacionCommand cmd, CancellationToken ct)
    {
        var reparacion = await _contexto.Reparacions
            .Include(r => r.Factura)
            .FirstOrDefaultAsync(r => r.ReparacionId == cmd.ReparacionId, ct);

        if (reparacion is null)
            return Resultado.Fallo("Reparacion no encontrada.");

        if (reparacion.Factura.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede modificar una reparacion de una factura Pagada.");

        var diferencia = cmd.Costo - reparacion.Costo;

        reparacion.DescripcionReparacion   = cmd.DescripcionReparacion;
        reparacion.Costo                   = cmd.Costo;
        reparacion.DuracionAproximadaHoras = cmd.DuracionAproximadaHoras;

        var factura = reparacion.Factura;
        factura.TotalReparaciones += diferencia;
        var subtotal  = factura.TotalRepuestos + factura.TotalReparaciones - factura.Descuento;
        factura.Total = subtotal + subtotal * factura.ImpuestoVentas / 100m;

        await RecalculadorFechaSalidaFactura.RecalcularFechaSalidaAsync(
            _contexto, factura.FacturaId, cmd.DuracionAproximadaHoras, cmd.ReparacionId, ct);

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
