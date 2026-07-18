using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Facturas.Commands.CrearFactura;

public class CrearFacturaHandler : IRequestHandler<CrearFacturaCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearFacturaHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearFacturaCommand cmd, CancellationToken ct)
    {
        var vehiculo = await _contexto.Vehiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.VehiculoId == cmd.VehiculoId, ct);

        if (vehiculo is null)
            return Resultado<int>.Fallo("Vehiculo no encontrado.");

        var factura = new Factura
        {
            VehiculoId         = cmd.VehiculoId,
            Fecha              = cmd.Fecha,
            NombreCliente      = vehiculo.Cliente.NombreCliente,
            DescripcionGeneral = cmd.DescripcionGeneral,
            TotalRepuestos     = 0,
            TotalReparaciones  = 0,
            Descuento          = cmd.Descuento,
            Adelanto           = cmd.Adelanto,
            EstadoFacturaId    = 1
        };

        await RecalculadorTotalesFactura.RecalcularAsync(_contexto, factura, ct);

        await _contexto.Facturas.AddAsync(factura, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(factura.FacturaId);
    }
}
