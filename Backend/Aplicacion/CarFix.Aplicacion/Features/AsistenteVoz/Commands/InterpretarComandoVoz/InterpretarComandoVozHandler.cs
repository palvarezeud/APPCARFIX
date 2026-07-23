using CarFix.Aplicacion.Comun;
using CarFix.Aplicacion.Features.AsistenteVoz.Dtos;
using CarFix.Dominio.Interfaces;
using MediatR;

namespace CarFix.Aplicacion.Features.AsistenteVoz.Commands.InterpretarComandoVoz;

public class InterpretarComandoVozHandler
    : IRequestHandler<InterpretarComandoVozCommand, Resultado<InterpretacionVozDto>>
{
    private readonly IServicioAsistenteVoz _servicioAsistenteVoz;

    public InterpretarComandoVozHandler(IServicioAsistenteVoz servicioAsistenteVoz)
        => _servicioAsistenteVoz = servicioAsistenteVoz;

    public async Task<Resultado<InterpretacionVozDto>> Handle(
        InterpretarComandoVozCommand cmd, CancellationToken ct)
    {
        var resultado = await _servicioAsistenteVoz.InterpretarAsync(
            cmd.Transcripcion, cmd.PantallaActual, cmd.IntentEnProgreso, ct);

        if (!resultado.EsExitoso)
            return Resultado<InterpretacionVozDto>.Fallo(
                resultado.MensajeError ?? "No se pudo interpretar el comando.");

        var cliente = resultado.Cliente is null ? null : new ClienteVozDto(
            resultado.Cliente.NombreCliente, resultado.Cliente.Telefono1,
            resultado.Cliente.Telefono2, resultado.Cliente.Email, resultado.Cliente.EsEmpresa);

        var vehiculo = resultado.Vehiculo is null ? null : new VehiculoVozDto(
            resultado.Vehiculo.Placa, resultado.Vehiculo.Marca, resultado.Vehiculo.Modelo,
            resultado.Vehiculo.Vin, resultado.Vehiculo.Annio, resultado.Vehiculo.Motor,
            resultado.Vehiculo.EsAutomatico, resultado.Vehiculo.NombreClienteBuscado);

        var orden = resultado.Orden is null ? null : new OrdenVozDto(
            resultado.Orden.ProblemaGeneral, resultado.Orden.EsGarantia, resultado.Orden.PlacaBuscada);

        var factura = resultado.Factura is null ? null : new FacturaVozDto(
            resultado.Factura.NombreClienteBuscado, resultado.Factura.PlacaBuscada);

        var reparacion = resultado.Reparacion is null ? null : new ReparacionVozDto(
            resultado.Reparacion.DescripcionReparacion, resultado.Reparacion.Costo,
            resultado.Reparacion.DuracionAproximadaHoras);

        var repuesto = resultado.Repuesto is null ? null : new RepuestoVozDto(
            resultado.Repuesto.NombreRepuesto, resultado.Repuesto.Costo,
            resultado.Repuesto.Repuestera, resultado.Repuesto.NumeroFactura);

        var dto = new InterpretacionVozDto(
            resultado.Intent ?? "desconocido",
            resultado.PantallaDestino,
            resultado.AbrirFormularioCrear,
            resultado.TerminoBusqueda,
            cliente,
            vehiculo,
            orden,
            factura,
            reparacion,
            repuesto,
            resultado.MensajeParaUsuario);

        return Resultado<InterpretacionVozDto>.Exito(dto);
    }
}
