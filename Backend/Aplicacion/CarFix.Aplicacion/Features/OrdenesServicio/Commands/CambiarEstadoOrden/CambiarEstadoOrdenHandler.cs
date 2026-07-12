using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CambiarEstadoOrden;

public class CambiarEstadoOrdenHandler : IRequestHandler<CambiarEstadoOrdenCommand, Resultado>
{
    private readonly IRepositorioOrdenServicio _repositorio;
    private readonly ICarFixDbContext          _contexto;
    private readonly IUnidadTrabajo            _unidadTrabajo;

    public CambiarEstadoOrdenHandler(
        IRepositorioOrdenServicio repositorio,
        ICarFixDbContext          contexto,
        IUnidadTrabajo            unidadTrabajo)
    {
        _repositorio   = repositorio;
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(CambiarEstadoOrdenCommand cmd, CancellationToken ct)
    {
        var orden = await _repositorio.ObtenerPorIdAsync(cmd.OrdenServicioId, ct);
        if (orden is null)
            return Resultado.Fallo("Orden de servicio no encontrada.");

        var estadoValido = await _contexto.EstadoOrdens
            .AnyAsync(e => e.EstadoOrdenId == cmd.NuevoEstadoId, ct);

        if (!estadoValido)
            return Resultado.Fallo("Estado de orden no valido.");

        orden.EstadoOrdenId = cmd.NuevoEstadoId;
        _repositorio.Actualizar(orden);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}
