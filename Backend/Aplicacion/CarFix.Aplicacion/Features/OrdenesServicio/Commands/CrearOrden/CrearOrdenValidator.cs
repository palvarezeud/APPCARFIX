using FluentValidation;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.CrearOrden;

public class CrearOrdenValidator : AbstractValidator<CrearOrdenCommand>
{
    public CrearOrdenValidator()
    {
        RuleFor(x => x.VehiculoId).GreaterThan(0).WithMessage("El vehiculo es requerido.");
        RuleFor(x => x.ProblemaGeneral).NotEmpty().WithMessage("El problema general es requerido.");
        RuleFor(x => x.FechaIngreso).NotEmpty().WithMessage("La fecha de ingreso es requerida.");
        RuleFor(x => x.FechaSalida).GreaterThan(x => x.FechaIngreso)
            .WithMessage("La fecha de salida debe ser posterior a la fecha de ingreso.");
    }
}
