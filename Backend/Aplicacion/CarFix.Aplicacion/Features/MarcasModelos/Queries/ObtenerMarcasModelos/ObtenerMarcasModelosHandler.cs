using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.MarcasModelos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.MarcasModelos.Queries.ObtenerMarcasModelos;

public class ObtenerMarcasModelosHandler
    : IRequestHandler<ObtenerMarcasModelosQuery, Resultado<IEnumerable<MarcaModeloDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerMarcasModelosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<MarcaModeloDto>>> Handle(
        ObtenerMarcasModelosQuery query, CancellationToken ct)
    {
        var resultado = await _contexto.MarcaModelos
            .OrderBy(mm => mm.Marca)
            .ThenBy(mm => mm.Modelo)
            .ThenBy(mm => mm.Annio)
            .Select(mm => new MarcaModeloDto(mm.MarcaModeloId, mm.Marca, mm.Modelo, mm.Annio))
            .ToListAsync(ct);

        return Resultado<IEnumerable<MarcaModeloDto>>.Exito(resultado);
    }
}
