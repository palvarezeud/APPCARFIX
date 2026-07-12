using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.HistoricoRepuestos.Commands.CrearHistoricoRepuesto;

public class CrearHistoricoRepuestoHandler : IRequestHandler<CrearHistoricoRepuestoCommand, Resultado<int>>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public CrearHistoricoRepuestoHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearHistoricoRepuestoCommand cmd, CancellationToken ct)
    {
        var registro = new HistoricoRespuesto
        {
            Marca               = cmd.Marca,
            Modelo              = cmd.Modelo,
            Annio               = cmd.Annio,
            RepuestoDecripcion  = cmd.RepuestoDecripcion,
            Precio              = cmd.Precio,
            Repuestera          = cmd.Repuestera,
            FechaCompra         = cmd.FechaCompra
        };

        _contexto.HistoricoRespuestos.Add(registro);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(registro.RespuestoHistoricoId);
    }
}
