using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.EliminarTipoReparacion;

public class EliminarTipoReparacionHandler : IRequestHandler<EliminarTipoReparacionCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarTipoReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarTipoReparacionCommand cmd, CancellationToken ct)
    {
        var tipo = await _contexto.TipoReparacions
            .FirstOrDefaultAsync(t => t.TipoReparacionId == cmd.TipoReparacionId, ct);

        if (tipo is null)
            return Resultado.Fallo("Tipo de reparacion no encontrado.");

        _contexto.TipoReparacions.Remove(tipo);
        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
