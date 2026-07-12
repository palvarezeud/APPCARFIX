using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace CarFix.Aplicacion.Features.Talleres.Commands.ActualizarTaller;

public class ActualizarTallerHandler : IRequestHandler<ActualizarTallerCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarTallerHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarTallerCommand cmd, CancellationToken ct)
    {
        var taller = await _contexto.Tallers.FirstOrDefaultAsync(ct);

        if (taller is null)
            return Resultado.Fallo("No hay datos del taller registrados.");

        taller.Nombre               = cmd.Nombre;
        taller.UbicacionDescripcion = cmd.UbicacionDescripcion;
        taller.Telefonos            = cmd.Telefonos;
        taller.Email                = cmd.Email;
        taller.UbicacionGps         = cmd.Latitud.HasValue && cmd.Longitud.HasValue
            ? new Point((double)cmd.Longitud.Value, (double)cmd.Latitud.Value) { SRID = 4326 }
            : null;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
