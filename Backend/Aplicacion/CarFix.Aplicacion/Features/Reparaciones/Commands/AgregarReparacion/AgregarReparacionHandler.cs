using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Reparaciones.Commands.AgregarReparacion;

public class AgregarReparacionHandler : IRequestHandler<AgregarReparacionCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public AgregarReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(AgregarReparacionCommand cmd, CancellationToken ct)
    {
        var facturaExiste = await _contexto.Facturas
            .AnyAsync(f => f.FacturaId == cmd.FacturaId, ct);

        if (!facturaExiste)
            return Resultado<int>.Fallo("Factura no encontrada.");

        var reparacion = new Reparacion
        {
            FacturaId                = cmd.FacturaId,
            DescripcionReparacion    = cmd.DescripcionReparacion,
            Costo                    = cmd.Costo,
            Listo                    = false,
            DuracionAproximadaHoras  = cmd.DuracionAproximadaHoras
        };

        await _contexto.Reparacions.AddAsync(reparacion, ct);

        var factura = await _contexto.Facturas.FindAsync([cmd.FacturaId], ct);
        factura!.TotalReparaciones += cmd.Costo;

        await RecalculadorTotalesFactura.RecalcularAsync(_contexto, factura, ct);

        await RecalculadorFechaSalidaFactura.RecalcularFechaSalidaAsync(
            _contexto, cmd.FacturaId, cmd.DuracionAproximadaHoras, null, ct);

        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(reparacion.ReparacionId);
    }
}
