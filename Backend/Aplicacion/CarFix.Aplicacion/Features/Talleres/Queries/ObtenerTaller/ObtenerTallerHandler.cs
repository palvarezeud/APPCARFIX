using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Talleres.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Talleres.Queries.ObtenerTaller;

public class ObtenerTallerHandler : IRequestHandler<ObtenerTallerQuery, Resultado<TallerDto>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerTallerHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<TallerDto>> Handle(ObtenerTallerQuery query, CancellationToken ct)
    {
        var taller = await _contexto.Tallers.FirstOrDefaultAsync(ct);

        if (taller is null)
            return Resultado<TallerDto>.Fallo("No hay datos del taller registrados.");

        var dto = new TallerDto(
            taller.TallerId,
            taller.Nombre,
            taller.UbicacionDescripcion,
            taller.Telefonos,
            taller.Email,
            taller.UbicacionGps is null ? null : (decimal)taller.UbicacionGps.Y,
            taller.UbicacionGps is null ? null : (decimal)taller.UbicacionGps.X);

        return Resultado<TallerDto>.Exito(dto);
    }
}
