using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.ActualizarHistoricoRepuesto;

public class ActualizarHistoricoRepuestoHandler : IRequestHandler<ActualizarHistoricoRepuestoCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarHistoricoRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarHistoricoRepuestoCommand cmd, CancellationToken ct)
    {
        var registro = await _contexto.HistoricoRespuestos
            .FirstOrDefaultAsync(h => h.RespuestoHistoricoId == cmd.RespuestoHistoricoId, ct);

        if (registro is null)
            return Resultado.Fallo("Registro no encontrado.");

        registro.Marca              = cmd.Marca;
        registro.Modelo             = cmd.Modelo;
        registro.Annio              = cmd.Annio;
        registro.RepuestoDecripcion = cmd.RepuestoDecripcion;
        registro.Precio             = cmd.Precio;
        registro.Repuestera         = cmd.Repuestera;
        registro.FechaCompra        = cmd.FechaCompra;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
