using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.OrdenesServicio.Commands.CambiarEstadoOrden;
using CarFix.Dominio.Entidades;
using CarFix.Especificaciones.Soporte;
using CarFix.Infraestructura.Persistencia;
using FluentValidation;
using MediatR;
using Reqnroll;

namespace CarFix.Especificaciones.Steps;

[Binding]
public class OrdenServicioSteps
{
    private readonly ISender           _sender;
    private readonly CarFixDbContext   _contexto;
    private readonly ContextoEscenario _contextoEscenario;

    private int       _ordenActualId;
    private Resultado? _resultado;

    public OrdenServicioSteps(ISender sender, CarFixDbContext contexto, ContextoEscenario contextoEscenario)
    {
        _sender            = sender;
        _contexto          = contexto;
        _contextoEscenario = contextoEscenario;
    }

    [Given(@"ese cliente tiene un vehiculo ""(.*)"" ""(.*)""")]
    public void DadoClienteTieneVehiculo(string marca, string modelo)
    {
        var cliente = _contexto.Clientes.First();
        var vehiculo = new Vehiculo
        {
            Marca              = marca,
            Modelo             = modelo,
            DetallesCarroceria = "Sin golpes",
            EsAutomatico       = false,
            ClienteId          = cliente.ClienteId
        };
        _contexto.Vehiculos.Add(vehiculo);
        _contexto.SaveChanges();
    }

    [Given(@"ese vehiculo tiene una orden de servicio en estado ""(.*)""")]
    public void DadoVehiculoTieneOrden(string estadoNombre)
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

        var estadoId = _contexto.EstadoOrdens
            .First(e => e.Descripcion == estadoNombre.Replace("ó", "o"))
            .EstadoOrdenId;

        var orden = new OrdenServicio
        {
            VehiculoId      = vehiculo.VehiculoId,
            FacturaId       = factura.FacturaId,
            FechaIngreso    = DateTime.UtcNow,
            FechaSalida     = DateTime.UtcNow.AddDays(3),
            ProblemaGeneral = "Revision general",
            EstadoOrdenId   = estadoId,
            EsGarantia      = false
        };
        _contexto.OrdenServicios.Add(orden);
        _contexto.SaveChanges();

        _ordenActualId = orden.OrdenServicioId;
    }

    [When(@"cambio el estado de la orden a (\d+)")]
    public async Task CuandoCambioEstadoOrden(int nuevoEstadoId)
    {
        try
        {
            _resultado = await _sender.Send(new CambiarEstadoOrdenCommand(_ordenActualId, nuevoEstadoId));
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [Then(@"la orden tiene estado (\d+)")]
    public void EntoncesOrdenTieneEstado(int estadoEsperado)
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultado!.EsExitoso, Is.True);
        var orden = _contexto.OrdenServicios.Find(_ordenActualId);
        Assert.That(orden!.EstadoOrdenId, Is.EqualTo(estadoEsperado));
    }
}
