using FluentValidation;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CambiarContrasenna;

public class CambiarContrasennaValidator : AbstractValidator<CambiarContrasennaCommand>
{
    public CambiarContrasennaValidator()
    {
        RuleFor(x => x.NuevoPassword)
            .NotEmpty().WithMessage("La nueva contrasenna es requerida.")
            .MinimumLength(6).WithMessage("La contrasenna debe tener al menos 6 caracteres.");
    }
}
