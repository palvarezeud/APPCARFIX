using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.AgregarRepuesto;

public class AgregarRepuestoHandler : IRequestHandler<AgregarRepuestoCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public AgregarRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(AgregarRepuestoCommand cmd, CancellationToken ct)
    {
        var facturaExiste = await _contexto.Facturas
            .AnyAsync(f => f.FacturaId == cmd.FacturaId, ct);

        if (!facturaExiste)
            return Resultado<int>.Fallo("Factura no encontrada.");

        var repuesto = new Repuesto
        {
            FacturaId     = cmd.FacturaId,
            NombreRepuesto = cmd.NombreRepuesto,
            Costo         = cmd.Costo,
            Fecha         = cmd.Fecha,
            Repuestera    = cmd.Repuestera,
            Factura       = cmd.NumeroFactura
        };

        await _contexto.Repuestos.AddAsync(repuesto, ct);

        var factura = await _contexto.Facturas.FindAsync([cmd.FacturaId], ct);
        factura!.TotalRepuestos += cmd.Costo;
        factura.Total           =  factura.TotalRepuestos + factura.TotalReparaciones - factura.Descuento;
        factura.Total           += factura.Total * factura.ImpuestoVentas / 100m;

        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(repuesto.RepuestoId);
    }
}
