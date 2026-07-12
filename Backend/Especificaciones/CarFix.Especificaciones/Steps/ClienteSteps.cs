using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.Clientes.Commands.CrearCliente;
using CarFix.Aplicacion.Features.Clientes.Commands.EliminarCliente;
using CarFix.Dominio.Entidades;
using CarFix.Especificaciones.Soporte;
using CarFix.Infraestructura.Persistencia;
using FluentValidation;
using MediatR;
using Reqnroll;

namespace CarFix.Especificaciones.Steps;

[Binding]
public class ClienteSteps
{
    private readonly ISender           _sender;
    private readonly CarFixDbContext   _contexto;
    private readonly ContextoEscenario _contextoEscenario;

    private Resultado<int>? _resultadoCrear;
    private Resultado?      _resultadoEliminar;
    private int             _clienteActualId;

    public ClienteSteps(ISender sender, CarFixDbContext contexto, ContextoEscenario contextoEscenario)
    {
        _sender            = sender;
        _contexto          = contexto;
        _contextoEscenario = contextoEscenario;
    }

    [When(@"creo un cliente con nombre ""(.*)"" y telefono ""(.*)""")]
    public async Task CuandoCreoCliente(string nombre, string telefono)
    {
        try
        {
            _resultadoCrear = await _sender.Send(new CrearClienteCommand(nombre, telefono, null, null, false));
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [When(@"creo un cliente sin nombre y telefono ""(.*)""")]
    public async Task CuandoCreoClienteSinNombre(string telefono)
    {
        try
        {
            _resultadoCrear = await _sender.Send(new CrearClienteCommand("", telefono, null, null, false));
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [When(@"creo un cliente con nombre ""(.*)"" y sin telefono")]
    public async Task CuandoCreoClienteSinTelefono(string nombre)
    {
        try
        {
            _resultadoCrear = await _sender.Send(new CrearClienteCommand(nombre, "", null, null, false));
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [Given(@"existe un cliente ""(.*)"" con telefono ""(.*)""")]
    public async Task DadoExisteCliente(string nombre, string telefono)
    {
        var resultado = await _sender.Send(new CrearClienteCommand(nombre, telefono, null, null, false));
        _clienteActualId = resultado.Valor!;
    }

    [Given(@"ese cliente tiene una factura registrada")]
    public void DadoClienteTieneFactura()
    {
        var vehiculo = new Vehiculo
        {
            Marca              = "Toyota",
            DetallesCarroceria = "Sin golpes",
            EsAutomatico       = false,
            ClienteId          = _clienteActualId
        };
        _contexto.Vehiculos.Add(vehiculo);
        _contexto.SaveChanges();

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
    }

    [When(@"elimino el cliente ""(.*)""")]
    public async Task CuandoEliminoCliente(string nombre)
    {
        var cliente = _contexto.Clientes.First(c => c.NombreCliente == nombre);
        try
        {
            _resultadoEliminar = await _sender.Send(new EliminarClienteCommand(cliente.ClienteId));
        }
        catch (ValidationException ex)
        {
            _contextoEscenario.ErrorValidacion = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        }
    }

    [Then(@"el cliente fue creado con ID mayor a cero")]
    public void EntoncesClienteFueCreado()
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultadoCrear!.EsExitoso, Is.True);
        Assert.That(_resultadoCrear.Valor, Is.GreaterThan(0));
    }

    [Then(@"la operacion fue exitosa")]
    public void EntoncesOperacionExitosa()
    {
        Assert.That(_contextoEscenario.ErrorValidacion, Is.Null,
            $"Hubo error de validacion: {_contextoEscenario.ErrorValidacion}");
        Assert.That(_resultadoEliminar!.EsExitoso, Is.True);
    }

    [Then(@"la operacion falla con ""(.*)""")]
    public void EntoncesOperacionFalla(string mensajeEsperado)
    {
        if (_contextoEscenario.ErrorValidacion is not null)
        {
            Assert.That(_contextoEscenario.ErrorValidacion, Does.Contain(mensajeEsperado));
            return;
        }
        Assert.That(_resultadoEliminar!.EsExitoso, Is.False);
        Assert.That(_resultadoEliminar.Error, Is.EqualTo(mensajeEsperado));
    }
}
