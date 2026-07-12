using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Clientes.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Clientes.Queries.ObtenerClientes;

public class ObtenerClientesHandler : IRequestHandler<ObtenerClientesQuery, Resultado<IEnumerable<ClienteDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerClientesHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<ClienteDto>>> Handle(ObtenerClientesQuery query, CancellationToken ct)
    {
        var q = _contexto.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filtro))
            q = q.Where(c => c.NombreCliente.Contains(query.Filtro));

        var resultado = await q
            .OrderBy(c => c.ClienteId)
            .Select(c => new ClienteDto(
                c.ClienteId,
                c.NombreCliente,
                c.Telefono1,
                c.Telefono2,
                c.Email,
                c.EsEmpresa))
            .ToListAsync(ct);

        return Resultado<IEnumerable<ClienteDto>>.Exito(resultado);
    }
}
