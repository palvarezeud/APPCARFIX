using FluentValidation;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.RefrescarSesion;

public class RefrescarSesionValidator : AbstractValidator<RefrescarSesionCommand>
{
    public RefrescarSesionValidator()
    {
        RuleFor(x => x.TokenRefresco)
            .NotEmpty().WithMessage("El token de refresco es requerido.");
    }
}
