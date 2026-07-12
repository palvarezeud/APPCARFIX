using FluentValidation;

namespace CarFix.Aplicacion.Features.Parametros.Commands.ActualizarParametro;

public class ActualizarParametroValidator : AbstractValidator<ActualizarParametroCommand>
{
    public ActualizarParametroValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del parametro es requerido.")
            .MaximumLength(200);

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor del parametro es requerido.");
    }
}
