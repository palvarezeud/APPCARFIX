using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Comun;

// Recalcula la FechaSalida de la orden asociada a una factura sumando las
// horas de reparacion conocidas a la FechaIngreso (saltando fines de semana).
// Si el total de horas conocidas es 0, se deja la FechaSalida sin tocar.
public static class RecalculadorFechaSalidaFactura
{
    public static async Task RecalcularFechaSalidaAsync(
        ICarFixDbContext contexto,
        int facturaId,
        int? duracionNueva,
        int? reparacionIdExcluir,
        CancellationToken ct)
    {
        var totalHoras = await contexto.Reparacions
            .Where(r => r.FacturaId == facturaId
                        && r.ReparacionId != (reparacionIdExcluir ?? 0)
                        && r.DuracionAproximadaHoras.HasValue)
            .SumAsync(r => r.DuracionAproximadaHoras!.Value, ct);

        totalHoras += duracionNueva ?? 0;

        if (totalHoras <= 0)
            return;

        var orden = await contexto.OrdenServicios.FirstOrDefaultAsync(o => o.FacturaId == facturaId, ct);
        if (orden is null)
            return;

        orden.FechaSalida = CalculadoraFechaSalida.Calcular(orden.FechaIngreso, totalHoras);
    }
}
