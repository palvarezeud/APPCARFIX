using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Repuestos.Commands.MarcarIncluidoRepuesto;

public class MarcarIncluidoRepuestoHandler : IRequestHandler<MarcarIncluidoRepuestoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public MarcarIncluidoRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(MarcarIncluidoRepuestoCommand cmd, CancellationToken ct)
    {
        var repuesto = await _contexto.Repuestos
            .Include(r => r.FacturaNavigation)
            .FirstOrDefaultAsync(r => r.RepuestoId == cmd.RepuestoId, ct);

        if (repuesto is null)
            return Resultado.Fallo("Repuesto no encontrado.");

        if (repuesto.FacturaNavigation.EstadoFacturaId == 3)
            return Resultado.Fallo("No se puede modificar un repuesto de una factura Pagada.");

        repuesto.Incluido = cmd.Incluido;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
