using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Entidades;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.Clientes.Commands.CrearCliente;

public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, Resultado<int>>
{
    private readonly IRepositorioCliente _repositorio;
    private readonly IUnidadTrabajo      _unidadTrabajo;

    public CrearClienteHandler(IRepositorioCliente repositorio, IUnidadTrabajo unidadTrabajo)
    {
        _repositorio   = repositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado<int>> Handle(CrearClienteCommand cmd, CancellationToken ct)
    {
        var cliente = new Cliente
        {
            NombreCliente = cmd.NombreCliente,
            Telefono1     = cmd.Telefono1,
            Telefono2     = cmd.Telefono2,
            Email         = cmd.Email,
            EsEmpresa     = cmd.EsEmpresa
        };

        await _repositorio.AgregarAsync(cliente, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado<int>.Exito(cliente.ClienteId);
    }
}
