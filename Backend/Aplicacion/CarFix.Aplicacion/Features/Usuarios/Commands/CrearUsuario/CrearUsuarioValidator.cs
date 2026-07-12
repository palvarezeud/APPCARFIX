using FluentValidation;

namespace CarFix.Aplicacion.Features.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioValidator()
    {
        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasenna es requerida.")
            .MinimumLength(6).WithMessage("La contrasenna debe tener al menos 6 caracteres.");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.RolId)
            .GreaterThan(0).WithMessage("El rol es requerido.");
    }
}
