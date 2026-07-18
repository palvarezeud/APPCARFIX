using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.OrdenesServicio.Dtos;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CrearOrden;

public class CrearOrdenHandler : IRequestHandler<CrearOrdenCommand, Resultado<CrearOrdenResponseDto>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearOrdenHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<CrearOrdenResponseDto>> Handle(CrearOrdenCommand cmd, CancellationToken ct)
    {
        var vehiculo = await _contexto.Vehiculos
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.VehiculoId == cmd.VehiculoId, ct);

        if (vehiculo is null)
            return Resultado<CrearOrdenResponseDto>.Fallo("El vehiculo no existe.");

        var factura = new Factura
        {
            VehiculoId         = cmd.VehiculoId,
            Fecha              = DateTime.Now,
            NombreCliente      = vehiculo.Cliente.NombreCliente,
            DescripcionGeneral = string.Empty,
            TotalRepuestos     = 0,
            TotalReparaciones  = 0,
            Descuento          = 0,
            Adelanto           = 0,
            EstadoFacturaId    = 1 // Cotizacion
        };

        await RecalculadorTotalesFactura.RecalcularAsync(_contexto, factura, ct);

        var orden = new OrdenServicio
        {
            VehiculoId      = cmd.VehiculoId,
            Factura         = factura, // EF resuelve el FK en un solo commit
            FechaIngreso    = cmd.FechaIngreso,
            FechaSalida     = cmd.FechaSalida,
            ProblemaGeneral = cmd.ProblemaGeneral.Trim(),
            EstadoOrdenId   = 1, // Cotizacion
            EsGarantia      = cmd.EsGarantia
        };

        await _contexto.OrdenServicios.AddAsync(orden, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct); // un solo commit atomico

        return Resultado<CrearOrdenResponseDto>.Exito(
            new CrearOrdenResponseDto(orden.OrdenServicioId, orden.FacturaId));
    }
}
