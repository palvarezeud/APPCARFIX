using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.EliminarOrden;

public class EliminarOrdenHandler : IRequestHandler<EliminarOrdenCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarOrdenHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarOrdenCommand cmd, CancellationToken ct)
    {
        var orden = await _contexto.OrdenServicios
            .Include(o => o.Factura)
            .FirstOrDefaultAsync(o => o.OrdenServicioId == cmd.OrdenServicioId, ct);

        if (orden is null)
            return Resultado.Fallo("Orden no encontrada.");

        if (orden.EstadoOrdenId is 4 or 5)
            return Resultado.Fallo("No se puede eliminar una orden en estado Finalizado o Entregado.");

        if (orden.Factura is not null && orden.Factura.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede eliminar una orden con factura en estado Pagada.");

        // Eliminar primero la orden (dependiente) y luego la factura (principal): la FK
        // OrdenServicio.FacturaId es obligatoria, por lo que EF Core no permite eliminar
        // la Factura mientras la Orden que la referencia siga sin marcarse para eliminar.
        var factura = orden.Factura;

        _contexto.OrdenServicios.Remove(orden);

        if (factura is not null)
            _contexto.Facturas.Remove(factura);

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
