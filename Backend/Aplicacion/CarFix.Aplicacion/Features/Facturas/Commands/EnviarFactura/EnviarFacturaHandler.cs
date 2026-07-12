using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Commands.EnviarFactura;

public class EnviarFacturaHandler : IRequestHandler<EnviarFacturaCommand, Resultado>
{
    private readonly ICarFixDbContext             _contexto;
    private readonly IServicioGeneradorFacturaPdf _generadorPdf;
    private readonly IServicioEnvioCorreo         _servicioCorreo;

    public EnviarFacturaHandler(
        ICarFixDbContext contexto,
        IServicioGeneradorFacturaPdf generadorPdf,
        IServicioEnvioCorreo servicioCorreo)
    {
        _contexto       = contexto;
        _generadorPdf   = generadorPdf;
        _servicioCorreo = servicioCorreo;
    }

    public async Task<Resultado> Handle(EnviarFacturaCommand cmd, CancellationToken ct)
    {
        var factura = await _contexto.Facturas
            .Include(f => f.Vehiculo).ThenInclude(v => v.Cliente)
            .Include(f => f.Reparaciones)
            .Include(f => f.Repuestos)
            .FirstOrDefaultAsync(f => f.FacturaId == cmd.FacturaId, ct);

        if (factura is null)
            return Resultado.Fallo("Factura no encontrada.");

        var emailCliente = factura.Vehiculo.Cliente.Email;
        if (string.IsNullOrWhiteSpace(emailCliente))
            return Resultado.Fallo(
                "El cliente no tiene correo electronico registrado. No se puede enviar la factura por este medio.");

        var taller = await _contexto.Tallers.FirstOrDefaultAsync(ct);
        if (taller is null)
            return Resultado.Fallo("No se encontraron los datos del taller. Contacte al administrador del sistema.");

        var pdfBytes = _generadorPdf.Generar(factura, taller);

        var nombreArchivo = $"Factura-{factura.FacturaId:D4}.pdf";
        var asunto        = $"{taller.Nombre} — Factura #{factura.FacturaId:D4}";
        var cuerpoHtml =
            $"<p>Estimado/a <strong>{factura.NombreCliente}</strong>,</p>" +
            $"<p>Adjunto encontrará la factura #{factura.FacturaId:D4} de {taller.Nombre}.</p>" +
            $"<p>Gracias por su preferencia.</p>";

        var resultadoEnvio = await _servicioCorreo.EnviarAsync(
            emailCliente, asunto, cuerpoHtml, pdfBytes, nombreArchivo, ct);

        return resultadoEnvio.EsExitoso
            ? Resultado.Exito()
            : Resultado.Fallo(resultadoEnvio.MensajeError!);
    }
}
