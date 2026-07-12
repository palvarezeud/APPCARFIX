using FluentValidation;

namespace CarFix.Aplicacion.Features.Parametros.Commands.CrearParametro;

public class CrearParametroValidator : AbstractValidator<CrearParametroCommand>
{
    public CrearParametroValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del parametro es requerido.")
            .MaximumLength(200);

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor del parametro es requerido.");
    }
}
