using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarFix.Infraestructura.Pdf;

// Genera el PDF de la factura siguiendo el diseno definido en CLAUDE.md seccion 6.
public class ServicioGeneradorFacturaPdfQuestPdf : IServicioGeneradorFacturaPdf
{
    public byte[] Generar(Factura factura, Taller taller)
    {
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter);
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Element(c => ComponerEncabezado(c, factura, taller));
                pagina.Content().Element(c => ComponerContenido(c, factura));
                pagina.Footer().AlignCenter().Text("CAR FIX — Sistema de Reparacion de Vehiculos").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });

        return documento.GeneratePdf();
    }

    private static void ComponerEncabezado(IContainer contenedor, Factura factura, Taller taller)
    {
        contenedor.Column(col =>
        {
            col.Item().Row(fila =>
            {
                fila.RelativeItem().Column(izq =>
                {
                    izq.Item().Text(taller.Nombre).FontSize(16).Bold();
                    izq.Item().Text(taller.UbicacionDescripcion).FontSize(9);
                    izq.Item().Text($"{taller.Telefonos} | {taller.Email}").FontSize(9);
                });
                fila.ConstantItem(160).Column(der =>
                {
                    der.Item().AlignRight().Text($"Factura #: {factura.FacturaId:D4}").Bold();
                    der.Item().AlignRight().Text($"Fecha: {factura.Fecha:dd/MM/yyyy}");
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            col.Item().PaddingTop(8).Text($"Cliente: {factura.NombreCliente}").Bold();
            col.Item().Text(
                $"Placa: {factura.Vehiculo.Placa ?? "—"}   " +
                $"Marca: {factura.Vehiculo.Marca}   " +
                $"Modelo: {factura.Vehiculo.Modelo ?? "—"}   " +
                $"Año: {(factura.Vehiculo.Annio.HasValue ? factura.Vehiculo.Annio.Value.ToString() : "—")}");
        });
    }

    private static void ComponerContenido(IContainer contenedor, Factura factura)
    {
        var repuestos    = factura.Repuestos.ToList();
        var reparaciones = factura.Reparaciones.ToList();
        var filas        = Math.Max(repuestos.Count, reparaciones.Count);

        contenedor.PaddingTop(15).Column(col =>
        {
            col.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.ConstantColumn(20);
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });

                tabla.Header(encabezado =>
                {
                    encabezado.Cell().Element(CeldaEncabezado).Text("Reparación");
                    encabezado.Cell().Element(CeldaEncabezado).AlignRight().Text("Monto");
                    encabezado.Cell().Element(CeldaEspaciadora);
                    encabezado.Cell().Element(CeldaEncabezado).Text("Repuesto");
                    encabezado.Cell().Element(CeldaEncabezado).AlignRight().Text("Precio");

                    static IContainer CeldaEncabezado(IContainer c) =>
                        c.DefaultTextStyle(x => x.Bold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Black);
                });

                if (filas == 0)
                {
                    tabla.Cell().ColumnSpan(5).Element(CeldaTexto).AlignCenter().Text("Sin reparaciones ni repuestos registrados.");
                }

                for (var i = 0; i < filas; i++)
                {
                    var repuesto   = i < repuestos.Count ? repuestos[i] : null;
                    var reparacion = i < reparaciones.Count ? reparaciones[i] : null;

                    tabla.Cell().Element(CeldaTexto).Text(reparacion?.DescripcionReparacion ?? "");
                    tabla.Cell().Element(CeldaTexto).AlignRight().Text(reparacion is not null ? FormatoColones(reparacion.Costo) : "");
                    tabla.Cell().Element(CeldaEspaciadora);
                    tabla.Cell().Element(CeldaTexto).Text(repuesto?.NombreRepuesto ?? "");
                    tabla.Cell().Element(CeldaTexto).AlignRight().Text(repuesto is not null ? FormatoColones(repuesto.Costo) : "");
                }

                static IContainer CeldaTexto(IContainer c) =>
                    c.PaddingVertical(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

                static IContainer CeldaEspaciadora(IContainer c) => c;
            });

            col.Item().PaddingTop(25).Row(fila =>
            {
                fila.RelativeItem().Column(izq =>
                {
                    izq.Item().PaddingTop(30).Width(180).LineHorizontal(1).LineColor(Colors.Black);
                    izq.Item().PaddingTop(2).Text("Recibido de conformidad").FontSize(9);
                });

                fila.ConstantItem(220).Column(der =>
                {
                    LineaTotal(der, "Total Repuestos:", factura.TotalRepuestos, false);
                    LineaTotal(der, "Total Reparaciones:", factura.TotalReparaciones, false);
                    LineaTotal(der, "Descuento:", factura.Descuento, false);
                    LineaTotal(der, "Impuesto de ventas:", factura.ImpuestoVentas, false);
                    der.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Black);
                    LineaTotal(der, "TOTAL GENERAL:", factura.Total, true);
                });
            });
        });
    }

    private static void LineaTotal(ColumnDescriptor columna, string etiqueta, decimal monto, bool destacado)
    {
        columna.Item().PaddingTop(2).Row(fila =>
        {
            if (destacado)
            {
                fila.RelativeItem().Text(etiqueta).Bold();
                fila.ConstantItem(90).AlignRight().Text(FormatoColones(monto)).Bold();
            }
            else
            {
                fila.RelativeItem().Text(etiqueta);
                fila.ConstantItem(90).AlignRight().Text(FormatoColones(monto));
            }
        });
    }

    private static string FormatoColones(decimal monto) => $"₡{monto:N2}";
}
