using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Commands.CambiarEstadoFactura;

public class CambiarEstadoFacturaHandler : IRequestHandler<CambiarEstadoFacturaCommand, Resultado>
{
    private readonly IRepositorioFactura _repositorio;
    private readonly ICarFixDbContext    _contexto;
    private readonly IUnidadTrabajo      _unidadTrabajo;

    public CambiarEstadoFacturaHandler(
        IRepositorioFactura repositorio,
        ICarFixDbContext    contexto,
        IUnidadTrabajo      unidadTrabajo)
    {
        _repositorio   = repositorio;
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(CambiarEstadoFacturaCommand cmd, CancellationToken ct)
    {
        var factura = await _repositorio.ObtenerPorIdAsync(cmd.FacturaId, ct);
        if (factura is null)
            return Resultado.Fallo("Factura no encontrada.");

        var estadoValido = await _contexto.EstadoFacturas
            .AnyAsync(e => e.EstadoFacturaId == cmd.NuevoEstadoId, ct);

        if (!estadoValido)
            return Resultado.Fallo("Estado de factura no valido.");

        // Al marcar como Pagada: copiar repuestos al historico
        if (cmd.NuevoEstadoId == 3 && factura.EstadoFacturaId != 3)
        {
            var detalle = await _contexto.Facturas
                .Include(f => f.Repuestos)
                .Include(f => f.Vehiculo)
                .FirstOrDefaultAsync(f => f.FacturaId == cmd.FacturaId, ct);

            if (detalle?.Vehiculo is not null && detalle.Repuestos.Count > 0)
            {
                var historicos = detalle.Repuestos.Select(r => new HistoricoRespuesto
                {
                    Marca              = detalle.Vehiculo.Marca,
                    Modelo             = detalle.Vehiculo.Modelo ?? string.Empty,
                    Annio              = (int)(detalle.Vehiculo.Annio ?? 0),
                    RepuestoDecripcion = r.NombreRepuesto,
                    Precio             = r.Costo,
                    Repuestera         = r.Repuestera,
                    FechaCompra        = r.Fecha
                });

                await _contexto.HistoricoRespuestos.AddRangeAsync(historicos, ct);
            }
        }

        factura.EstadoFacturaId = cmd.NuevoEstadoId;
        _repositorio.Actualizar(factura);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}
