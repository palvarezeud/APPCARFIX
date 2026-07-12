using CarFix.Aplicacion.Features.Facturas.Commands.CambiarEstadoFactura;
using CarFix.Aplicacion.Features.Reparaciones.Commands.AgregarReparacion;
using CarFix.Aplicacion.Features.Repuestos.Commands.AgregarRepuesto;
using CarFix.Dominio.Entidades;
using CarFix.Especificaciones.Soporte;
using CarFix.Infraestructura.Persistencia;
using FluentValidation;
using MediatR;
using Reqnroll;

namespace CarFix.Especificaciones.Steps;

[Binding]
public class FacturaSteps
{
    private readonly ISender           _sender;
    private readonly CarFixDbContext   _contexto;
    private readonly ContextoEscenario _contextoEscenario;

    private int     _facturaActualId;
    private bool    _resultadoExitoso;
    private string? _errorResultado;

    public FacturaSteps(ISender sender, CarFixDbContext contexto, ContextoEscenario contextoEscenario)
    {
        _sender            = sender;
        _contexto          = contexto;
        _contextoEscenario = contextoEscenario;
    }

    [Given(@"ese vehiculo tiene una factura en estado ""(.*)""")]
    public void DadoVehiculoTieneFactura(string estadoNombre)
    {
        var vehiculo = _contexto.Vehiculos.First();

        var factura = new Factura
        {
            VehiculoId         = vehiculo.VehiculoId,
            Fecha              = DateTime.UtcNow,
            NombreCliente      = "Test",
            DescripcionGeneral = "Factura de prueba",
            TotalRepuestos     = 0,
            TotalReparaciones  = 0,
            Total              = 0,
            Descuento          = 0,
            Adelanto           = 0,
            ImpuestoVentas     = 0,
            EstadoFacturaId    = 1
        };
        _contexto.Facturas.Add(factura);
        _contexto.SaveChanges();

        _facturaActualId = factura.FacturaId;
    }

    [When(@"cambio el estado de la factura a (\d+)")]
    public async Task CuandoCambioEstadoFactura(int nuevoEstadoId)
    {
        try
        {
            var r = await _sender.Send(new CambiarEstadoFacturaCommand(_facturaActualId, nuevoEstadoId));
            _resultadoExitoso = r.EsExitoso;
            _errorResultado   = r.Error;
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [When(@"agrego una reparacion ""(.*)"" con costo (\d+) a la factura")]
    public async Task CuandoAgregoReparacion(string descripcion, decimal costo)
    {
        var tipoReparacion = new TipoReparacion
        {
            TipoReparacionId        = 1,
            DescripcionReparacion   = descripcion,
            DuracionAproximadaHoras = 1,
            CostoBase               = costo
        };
        _contexto.TipoReparacions.Add(tipoReparacion);
        _contexto.SaveChanges();

        try
        {
            var r = await _sender.Send(new AgregarReparacionCommand(
                _facturaActualId, descripcion, costo));
            _resultadoExitoso = r.EsExitoso;
            _errorResultado   = r.Error;
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [When(@"agrego un repuesto ""(.*)"" con costo (\d+) a la factura")]
    public async Task CuandoAgregoRepuesto(string nombre, decimal costo)
    {
        try
        {
            var r = await _sender.Send(new AgregarRepuestoCommand(
                _facturaActualId,
                nombre,
                costo,
                DateTime.UtcNow,
                "Repuestera Prueba",
                "F-0001"));
            _resultadoExitoso = r.EsExitoso;
            _errorResultado   = r.Error;
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [Then(@"la factura tiene estado (\d+)")]
    public void EntoncesFacturaTieneEstado(int estadoEsperado)
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultadoExitoso, Is.True, $"La operacion fallo: {_errorResultado}");
        var factura = _contexto.Facturas.Find(_facturaActualId);
        Assert.That(factura!.EstadoFacturaId, Is.EqualTo(estadoEsperado));
    }

    [Then(@"la factura tiene total de reparaciones (\d+)")]
    public void EntoncesFacturaTieneTotalReparaciones(decimal totalEsperado)
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultadoExitoso, Is.True, $"La operacion fallo: {_errorResultado}");
        var factura = _contexto.Facturas.Find(_facturaActualId);
        Assert.That(factura!.TotalReparaciones, Is.EqualTo(totalEsperado));
    }

    [Then(@"la factura tiene total de repuestos (\d+)")]
    public void EntoncesFacturaTieneTotalRepuestos(decimal totalEsperado)
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultadoExitoso, Is.True, $"La operacion fallo: {_errorResultado}");
        var factura = _contexto.Facturas.Find(_facturaActualId);
        Assert.That(factura!.TotalRepuestos, Is.EqualTo(totalEsperado));
    }
}
