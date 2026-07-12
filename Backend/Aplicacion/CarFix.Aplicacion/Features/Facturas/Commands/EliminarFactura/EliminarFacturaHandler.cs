using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Commands.EliminarFactura;

public class EliminarFacturaHandler : IRequestHandler<EliminarFacturaCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarFacturaHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarFacturaCommand cmd, CancellationToken ct)
    {
        var factura = await _contexto.Facturas
            .Include(f => f.Reparaciones)
            .Include(f => f.Repuestos)
            .Include(f => f.OrdenServicio)
            .FirstOrDefaultAsync(f => f.FacturaId == cmd.FacturaId, ct);

        if (factura is null)
            return Resultado.Fallo("Factura no encontrada.");

        if (factura.EstadoFacturaId != 1)
            return Resultado.Fallo("Solo se pueden eliminar facturas en estado Cotizacion.");

        // Eliminar la orden de servicio asociada si existe
        if (factura.OrdenServicio is not null)
            _contexto.OrdenServicios.Remove(factura.OrdenServicio);

        // Eliminar reparaciones y repuestos
        _contexto.Reparacions.RemoveRange(factura.Reparaciones);
        _contexto.Repuestos.RemoveRange(factura.Repuestos);
        _contexto.Facturas.Remove(factura);

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
