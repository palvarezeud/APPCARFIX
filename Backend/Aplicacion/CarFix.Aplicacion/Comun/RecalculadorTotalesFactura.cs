using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Comun;

// Recalcula SubTotal, ImpuestoVentas (IVA), Total y Pendiente de una factura
// a partir de TotalRepuestos, TotalReparaciones, Descuento y Adelanto.
// La tasa de IVA se lee de Catalogo.Parametros (fila "ImpuestoVentas", ej. "13%").
public static class RecalculadorTotalesFactura
{
    private const string NombreParametroImpuesto = "ImpuestoVentas";

    public static async Task RecalcularAsync(ICarFixDbContext contexto, Factura factura, CancellationToken ct)
    {
        factura.SubTotal = factura.TotalRepuestos + factura.TotalReparaciones - factura.Descuento;

        var parametro = await contexto.Parametros
            .FirstOrDefaultAsync(p => p.Nombre == NombreParametroImpuesto, ct);

        var tasa = ParsearTasaPorcentaje(parametro?.Valor);

        factura.ImpuestoVentas = factura.SubTotal * tasa;
        factura.Total          = factura.SubTotal + factura.ImpuestoVentas;
        factura.Pendiente      = factura.Total - factura.Adelanto;
    }

    private static decimal ParsearTasaPorcentaje(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0m;

        var texto = valor.Trim().TrimEnd('%');
        return decimal.TryParse(texto, out var numero) ? numero / 100m : 0m;
    }
}
