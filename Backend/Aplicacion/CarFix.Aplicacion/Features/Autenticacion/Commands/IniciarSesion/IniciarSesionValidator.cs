using FluentValidation;

namespace CarFix.Aplicacion.Features.Autenticacion.Commands.IniciarSesion;

public class IniciarSesionValidator : AbstractValidator<IniciarSesionCommand>
{
    public IniciarSesionValidator()
    {
        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasenna es requerida.");
    }
}
