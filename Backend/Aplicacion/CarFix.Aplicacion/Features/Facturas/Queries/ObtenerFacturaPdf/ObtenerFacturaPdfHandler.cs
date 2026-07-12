using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Queries.ObtenerFacturaPdf;

public class ObtenerFacturaPdfHandler : IRequestHandler<ObtenerFacturaPdfQuery, Resultado<byte[]>>
{
    private readonly ICarFixDbContext             _contexto;
    private readonly IServicioGeneradorFacturaPdf _generadorPdf;

    public ObtenerFacturaPdfHandler(ICarFixDbContext contexto, IServicioGeneradorFacturaPdf generadorPdf)
    {
        _contexto     = contexto;
        _generadorPdf = generadorPdf;
    }

    public async Task<Resultado<byte[]>> Handle(ObtenerFacturaPdfQuery query, CancellationToken ct)
    {
        var factura = await _contexto.Facturas
            .Include(f => f.Vehiculo).ThenInclude(v => v.Cliente)
            .Include(f => f.Reparaciones)
            .Include(f => f.Repuestos)
            .FirstOrDefaultAsync(f => f.FacturaId == query.FacturaId, ct);

        if (factura is null)
            return Resultado<byte[]>.Fallo("Factura no encontrada.");

        var taller = await _contexto.Tallers.FirstOrDefaultAsync(ct);
        if (taller is null)
            return Resultado<byte[]>.Fallo("No se encontraron los datos del taller. Contacte al administrador del sistema.");

        var pdfBytes = _generadorPdf.Generar(factura, taller);

        return Resultado<byte[]>.Exito(pdfBytes);
    }
}
