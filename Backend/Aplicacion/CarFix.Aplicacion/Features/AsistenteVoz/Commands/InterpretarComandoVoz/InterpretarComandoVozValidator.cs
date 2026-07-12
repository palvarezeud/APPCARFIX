using FluentValidation;

namespace CarFix.Aplicacion.Features.AsistenteVoz.Commands.InterpretarComandoVoz;

public class InterpretarComandoVozValidator : AbstractValidator<InterpretarComandoVozCommand>
{
    public InterpretarComandoVozValidator()
    {
        RuleFor(x => x.Transcripcion)
            .NotEmpty().WithMessage("La transcripcion es requerida.")
            .MaximumLength(500).WithMessage("La transcripcion es demasiado larga.");
    }
}
