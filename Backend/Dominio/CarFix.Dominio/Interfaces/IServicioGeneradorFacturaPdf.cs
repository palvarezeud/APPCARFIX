using CarFix.Dominio.Entidades;

namespace CarFix.Dominio.Interfaces;

public interface IServicioGeneradorFacturaPdf
{
    byte[] Generar(Factura factura, Taller taller);
}
