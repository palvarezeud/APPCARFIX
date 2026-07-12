using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.ActualizarTipoReparacion;

public class ActualizarTipoReparacionHandler : IRequestHandler<ActualizarTipoReparacionCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarTipoReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarTipoReparacionCommand cmd, CancellationToken ct)
    {
        var tipo = await _contexto.TipoReparacions
            .FirstOrDefaultAsync(t => t.TipoReparacionId == cmd.TipoReparacionId, ct);

        if (tipo is null)
            return Resultado.Fallo("Tipo de reparacion no encontrado.");

        tipo.DescripcionReparacion   = cmd.DescripcionReparacion;
        tipo.DuracionAproximadaHoras = cmd.DuracionAproximadaHoras;
        tipo.CostoBase               = cmd.CostoBase;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
