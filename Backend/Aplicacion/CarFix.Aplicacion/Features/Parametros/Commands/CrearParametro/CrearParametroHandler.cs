using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Parametros.Commands.CrearParametro;

public class CrearParametroHandler : IRequestHandler<CrearParametroCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearParametroHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearParametroCommand cmd, CancellationToken ct)
    {
        var existe = await _contexto.Parametros
            .AnyAsync(p => p.Nombre == cmd.Nombre, ct);

        if (existe)
            return Resultado<int>.Fallo("Ya existe un parametro con ese nombre.");

        var parametro = new Parametro
        {
            Nombre = cmd.Nombre,
            Valor  = cmd.Valor
        };

        await _contexto.Parametros.AddAsync(parametro, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(parametro.ParametroId);
    }
}
