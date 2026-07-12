using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Vehiculos.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Vehiculos.Queries.ObtenerVehiculos;

public class ObtenerVehiculosHandler : IRequestHandler<ObtenerVehiculosQuery, Resultado<IEnumerable<VehiculoDto>>>
{
    private readonly ICarFixDbContext _contexto;

    public ObtenerVehiculosHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<IEnumerable<VehiculoDto>>> Handle(ObtenerVehiculosQuery query, CancellationToken ct)
    {
        var q = _contexto.Vehiculos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Filtro))
            q = q.Where(v => v.Placa!.Contains(query.Filtro) ||
                              v.Cliente.NombreCliente.Contains(query.Filtro));

        var resultado = await q
            .Include(v => v.Cliente)
            .OrderBy(v => v.VehiculoId)
            .Select(v => new VehiculoDto(
                v.VehiculoId,
                v.ClienteId,
                v.Cliente.NombreCliente,
                v.Placa,
                v.Marca,
                v.Modelo,
                v.Vin,
                v.Annio,
                v.Motor,
                v.EsAutomatico,
                v.DetallesCarroceria))
            .ToListAsync(ct);

        return Resultado<IEnumerable<VehiculoDto>>.Exito(resultado);
    }
}
