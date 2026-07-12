using CarFix.Aplicacion.Comun;
using CarFix.Dominio.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Aplicacion.Features.Salud.Queries.VerificarSalud;

public class VerificarSaludHandler : IRequestHandler<VerificarSaludQuery, Resultado<bool>>
{
    private readonly ICarFixDbContext _contexto;

    public VerificarSaludHandler(ICarFixDbContext contexto) => _contexto = contexto;

    public async Task<Resultado<bool>> Handle(VerificarSaludQuery query, CancellationToken ct)
    {
        // Consulta minima contra un catalogo pequenio: si la BD serverless esta pausada,
        // esta llamada dispara su resume (ver Connect Retry Count/Interval en el connection string).
        await _contexto.Roles.AnyAsync(ct);
        return Resultado<bool>.Exito(true);
    }
}
