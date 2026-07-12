using CarFix.Aplicacion.Comun;
using MediatR;

namespace CarFix.Aplicacion.Features.Salud.Queries.VerificarSalud;

public record VerificarSaludQuery() : IRequest<Resultado<bool>>;
