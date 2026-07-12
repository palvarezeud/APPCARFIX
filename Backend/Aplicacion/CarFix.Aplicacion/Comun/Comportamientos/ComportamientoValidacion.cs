using FluentValidation;
using MediatR;

namespace CarFix.Aplicacion.Comun.Comportamientos;

public class ComportamientoValidacion<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ComportamientoValidacion(IEnumerable<IValidator<TRequest>> validadores)
        => _validadores = validadores;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validadores.Any()) return await next(ct);

        var contexto = new ValidationContext<TRequest>(request);

        var errores = _validadores
            .Select(v => v.Validate(contexto))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (errores.Count != 0)
            throw new ValidationException(errores);

        return await next(ct);
    }
}
