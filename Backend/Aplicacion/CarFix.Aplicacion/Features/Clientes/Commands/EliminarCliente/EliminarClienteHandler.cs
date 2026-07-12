using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Excepciones;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Clientes.Commands.EliminarCliente;

public class EliminarClienteHandler : IRequestHandler<EliminarClienteCommand, Resultado>
{
    private readonly IRepositorioCliente _repositorio;
    private readonly ICarFixDbContext    _contexto;
    private readonly IUnidadTrabajo      _unidadTrabajo;

    public EliminarClienteHandler(
        IRepositorioCliente repositorio,
        ICarFixDbContext    contexto,
        IUnidadTrabajo      unidadTrabajo)
    {
        _repositorio   = repositorio;
        _contexto      = contexto;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<Resultado> Handle(EliminarClienteCommand cmd, CancellationToken ct)
    {
        var cliente = await _repositorio.ObtenerPorIdAsync(cmd.ClienteId, ct);
        if (cliente is null)
            return Resultado.Fallo("Cliente no encontrado.");

        var tieneFacturas = await _contexto.Facturas
            .AnyAsync(f => f.Vehiculo.ClienteId == cmd.ClienteId, ct);

        if (tieneFacturas)
            return Resultado.Fallo("No se puede eliminar un cliente que tiene facturas asociadas.");

        _repositorio.Eliminar(cliente);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        return Resultado.Exito();
    }
}
