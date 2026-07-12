using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Parametros.Commands.ActualizarParametro;

public class ActualizarParametroHandler : IRequestHandler<ActualizarParametroCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarParametroHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarParametroCommand cmd, CancellationToken ct)
    {
        var parametro = await _contexto.Parametros
            .FirstOrDefaultAsync(p => p.ParametroId == cmd.ParametroId, ct);

        if (parametro is null)
            return Resultado.Fallo("Parametro no encontrado.");

        var existeOtro = await _contexto.Parametros
            .AnyAsync(p => p.Nombre == cmd.Nombre && p.ParametroId != cmd.ParametroId, ct);

        if (existeOtro)
            return Resultado.Fallo("Ya existe otro parametro con ese nombre.");

        parametro.Nombre = cmd.Nombre;
        parametro.Valor  = cmd.Valor;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
