using FluentValidation;

namespace CarFix.Aplicacion.Features.Vehiculos.Commands.ActualizarVehiculo;

public class ActualizarVehiculoValidator : AbstractValidator<ActualizarVehiculoCommand>
{
    public ActualizarVehiculoValidator()
    {
        RuleFor(x => x.VehiculoId).GreaterThan(0);
        RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("El cliente es requerido.");
        RuleFor(x => x.Marca).NotEmpty().WithMessage("La marca es requerida.").MaximumLength(50);
        RuleFor(x => x.Modelo).NotEmpty().WithMessage("El modelo es requerido.").MaximumLength(50);
        RuleFor(x => x.Annio).InclusiveBetween((short)1900, (short)2100).WithMessage("El anno debe estar entre 1900 y 2100.");
        RuleFor(x => x.DetallesCarroceria).NotEmpty().WithMessage("Los detalles de carroceria son requeridos.");
    }
}
