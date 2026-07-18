using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Commands.ActualizarFactura;

public class ActualizarFacturaHandler : IRequestHandler<ActualizarFacturaCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarFacturaHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarFacturaCommand cmd, CancellationToken ct)
    {
        var factura = await _contexto.Facturas
            .FirstOrDefaultAsync(f => f.FacturaId == cmd.FacturaId, ct);

        if (factura is null)
            return Resultado.Fallo("Factura no encontrada.");

        if (factura.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede modificar una factura en estado Pagada.");

        factura.Fecha              = cmd.Fecha;
        factura.DescripcionGeneral = cmd.DescripcionGeneral ?? string.Empty;
        factura.Descuento          = cmd.Descuento;
        factura.Adelanto           = cmd.Adelanto;

        await RecalculadorTotalesFactura.RecalcularAsync(_contexto, factura, ct);

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
