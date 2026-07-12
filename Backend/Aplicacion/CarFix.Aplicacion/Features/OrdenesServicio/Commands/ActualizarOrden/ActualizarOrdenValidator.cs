using FluentValidation;

namespace CarFix.Aplicacion.Features.OrdenesServicio.Commands.ActualizarOrden;

public class ActualizarOrdenValidator : AbstractValidator<ActualizarOrdenCommand>
{
    public ActualizarOrdenValidator()
    {
        RuleFor(x => x.OrdenServicioId).GreaterThan(0);
        RuleFor(x => x.VehiculoId).GreaterThan(0).WithMessage("El vehiculo es requerido.");
        RuleFor(x => x.ProblemaGeneral).NotEmpty().WithMessage("El problema general es requerido.");
        RuleFor(x => x.FechaSalida).GreaterThan(x => x.FechaIngreso)
            .WithMessage("La fecha de salida debe ser posterior a la fecha de ingreso.");
        RuleFor(x => x.EstadoOrdenId).InclusiveBetween(1, 5).WithMessage("Estado de orden invalido.");
    }
}
