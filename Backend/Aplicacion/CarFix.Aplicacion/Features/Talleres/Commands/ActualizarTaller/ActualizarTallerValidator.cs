using FluentValidation;

namespace CarFix.Aplicacion.Features.Talleres.Commands.ActualizarTaller;

public class ActualizarTallerValidator : AbstractValidator<ActualizarTallerCommand>
{
    public ActualizarTallerValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del taller es requerido.");

        RuleFor(x => x.UbicacionDescripcion)
            .NotEmpty().WithMessage("La direccion del taller es requerida.");

        RuleFor(x => x.Telefonos)
            .NotEmpty().WithMessage("Los telefonos del taller son requeridos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo del taller es requerido.")
            .EmailAddress().WithMessage("El email no tiene un formato valido.");

        RuleFor(x => x.Latitud)
            .InclusiveBetween(-90, 90).When(x => x.Latitud.HasValue)
            .WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Longitud)
            .InclusiveBetween(-180, 180).When(x => x.Longitud.HasValue)
            .WithMessage("La longitud debe estar entre -180 y 180.");
    }
}
