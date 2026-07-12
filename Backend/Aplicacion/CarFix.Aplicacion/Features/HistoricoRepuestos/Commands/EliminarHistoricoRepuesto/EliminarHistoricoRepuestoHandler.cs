using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.EliminarHistoricoRepuesto;

public class EliminarHistoricoRepuestoHandler : IRequestHandler<EliminarHistoricoRepuestoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public EliminarHistoricoRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarHistoricoRepuestoCommand cmd, CancellationToken ct)
    {
        var registro = await _contexto.HistoricoRespuestos
            .FirstOrDefaultAsync(h => h.RespuestoHistoricoId == cmd.RespuestoHistoricoId, ct);

        if (registro is null)
            return Resultado.Fallo("Registro no encontrado.");

        _contexto.HistoricoRespuestos.Remove(registro);
        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
