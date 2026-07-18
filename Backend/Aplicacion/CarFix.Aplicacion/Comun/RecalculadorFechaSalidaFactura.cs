using System.Globalization;
using CarFix.Dominio.Excepciones;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Comun;

// Recalcula la FechaSalida de la orden asociada a una factura simulando el
// horario laboral real del taller (HoraApertura/HoraCierre en Catalogo.Parametros)
// a partir de la FechaIngreso, saltando fines de semana.
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

        var parametros = await contexto.Parametros
            .Where(p => p.Nombre == "HoraApertura" || p.Nombre == "HoraCierre")
            .ToListAsync(ct);

        var apertura = ParsearHora(parametros.FirstOrDefault(p => p.Nombre == "HoraApertura")?.Valor);
        var cierre   = ParsearHora(parametros.FirstOrDefault(p => p.Nombre == "HoraCierre")?.Valor);

        if (apertura is null || cierre is null || apertura >= cierre)
            throw new ExcepcionDominio("El horario del taller (HoraApertura/HoraCierre) no esta configurado correctamente en Catalogo.Parametros.");

        orden.FechaSalida = CalculadoraFechaSalida.Calcular(orden.FechaIngreso, totalHoras, apertura.Value, cierre.Value);
    }

    private static TimeSpan? ParsearHora(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return TimeSpan.TryParse(valor.Trim(), CultureInfo.InvariantCulture, out var hora) ? hora : null;
    }
}
