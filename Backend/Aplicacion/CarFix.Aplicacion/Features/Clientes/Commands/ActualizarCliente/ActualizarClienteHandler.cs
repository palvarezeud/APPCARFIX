using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Clientes.Commands.ActualizarCliente;

public class ActualizarClienteHandler : IRequestHandler<ActualizarClienteCommand, Resultado>
{
    private readonly ICarFixDbContext _contexto;
    private readonly IUnidadTrabajo  _unidadTrabajo;

    public ActualizarClienteHandler(ICarFixDbContext contexto, IUnidadTrabajo unidadTrabajo)
    {
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(ActualizarClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _contexto.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == cmd.ClienteId, ct);

        if (cliente is null)
            return Resultado.Fallo("Cliente no encontrado.");

        cliente.NombreCliente = cmd.NombreCliente;
        cliente.Telefono1     = cmd.Telefono1;
        cliente.Telefono2     = cmd.Telefono2;
        cliente.Email         = cmd.Email;
        cliente.EsEmpresa     = cmd.EsEmpresa;

        await _unidadTrabajo.GuardarCambiosAsync(ct);
        return Resultado.Exito();
    }
}
