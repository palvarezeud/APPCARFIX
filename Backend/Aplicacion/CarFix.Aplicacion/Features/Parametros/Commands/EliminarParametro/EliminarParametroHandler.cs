using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Parametros.Commands.EliminarParametro;

public class EliminarParametroHandler : IRequestHandler<EliminarParametroCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarParametroHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarParametroCommand cmd, CancellationToken ct)
    {
        var parametro = await _contexto.Parametros
            .FirstOrDefaultAsync(p => p.ParametroId == cmd.ParametroId, ct);

        if (parametro is null)
            return Resultado.Fallo("Parametro no encontrado.");

        _contexto.Parametros.Remove(parametro);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}
