using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Parametros.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Parametros.Queries.ObtenerParametros;

public class ObtenerParametrosHandler : IRequestHandler<ObtenerParametrosQuery, Resultado<IEnumerable<ParametroDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerParametrosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<ParametroDto>>> Handle(ObtenerParametrosQuery query, CancellationToken ct)
    {
        var resultado = await _contexto.Parametros
            .OrderBy(p => p.Nombre)
            .Select(p => new ParametroDto(p.ParametroId, p.Nombre, p.Valor))
            .ToListAsync(ct);

        return Resultado<IEnumerable<ParametroDto>>.Exito(resultado);
    }
}
