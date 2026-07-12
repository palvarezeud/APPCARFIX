using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.TiposReparacion.Commands.CrearTipoReparacion;

public class CrearTipoReparacionHandler : IRequestHandler<CrearTipoReparacionCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearTipoReparacionHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearTipoReparacionCommand cmd, CancellationToken ct)
    {
        var maxId = await _contexto.TipoReparacions
            .MaxAsync(t => (int?)t.TipoReparacionId, ct) ?? 0;

        var tipo = new TipoReparacion
        {
            TipoReparacionId        = maxId + 1,
            DescripcionReparacion   = cmd.DescripcionReparacion,
            DuracionAproximadaHoras = cmd.DuracionAproximadaHoras,
            CostoBase               = cmd.CostoBase
        };

        _contexto.TipoReparacions.Add(tipo);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(tipo.TipoReparacionId);
    }
}
