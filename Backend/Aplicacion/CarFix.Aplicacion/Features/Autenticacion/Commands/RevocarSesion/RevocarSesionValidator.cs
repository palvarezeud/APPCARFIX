using FluentValidation;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RevocarSesion;

public class RevocarSesionValidator : AbstractValidator<RevocarSesionCommand>
{
    public RevocarSesionValidator()
    {
        RuleFor(x => x.TokenRefresco)
            .NotEmpty().WithMessage("El token de refresco es requerido.");
    }
}
