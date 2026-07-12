using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena las ordenes de reparación de vehiculos
/// </summary>
public partial class OrdenServicio
{
    /// <summary>
    /// Identificador único de orden de reparación
    /// </summary>
    public int OrdenServicioId { get; set; }

    /// <summary>
    /// Vehículo asociado a la orden de reparación
    /// </summary>
    public int VehiculoId { get; set; }

    /// <summary>
    /// Fecha de ingreso del vehículo al taller
    /// </summary>
    public DateTime FechaIngreso { get; set; }

    /// <summary>
    /// Fecha de salida del vehículo del taller
    /// </summary>
    public DateTime FechaSalida { get; set; }

    /// <summary>
    /// Describe el problema o revisión que se le debe reparar al vehículo
    /// </summary>
    public string ProblemaGeneral { get; set; } = null!;

    /// <summary>
    /// Estado de la orden(0=Cotización,1=Recibido, 2=En revisión, 3= Finalizado, 4= Entregado
    /// </summary>
    public int EstadoOrdenId { get; set; }

    /// <summary>
    /// Especifica si la orden es para cubrir una garantía de un trabajo anterior
    /// </summary>
    public bool EsGarantia { get; set; }

    /// <summary>
    /// Asocia una factura a la orden de servicio
    /// </summary>
    public int FacturaId { get; set; }

    public virtual EstadoOrden EstadoOrden { get; set; } = null!;

    public virtual Factura Factura { get; set; } = null!;

    public virtual Vehiculo Vehiculo { get; set; } = null!;
}

