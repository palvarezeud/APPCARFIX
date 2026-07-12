using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena las facturas
/// </summary>
public partial class Factura
{
    /// <summary>
    /// Identificador único de la factura
    /// </summary>
    public int FacturaId { get; set; }

    /// <summary>
    /// Identificador del vehiculo asociado
    /// </summary>
    public int VehiculoId { get; set; }

    /// <summary>
    /// Fecha de la factura
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = null!;

    /// <summary>
    /// Descripción general de las reparaciones
    /// </summary>
    public string DescripcionGeneral { get; set; } = null!;

    /// <summary>
    /// Total de los repuestos comprados
    /// </summary>
    public decimal TotalRepuestos { get; set; }

    /// <summary>
    /// Total de las reparaciones realizadas
    /// </summary>
    public decimal TotalReparaciones { get; set; }

    /// <summary>
    /// Total de la factura 
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Si aplica algún descuento
    /// </summary>
    public decimal Descuento { get; set; }

    /// <summary>
    /// Si el cliente dio un adelanto de dinero 
    /// </summary>
    public decimal Adelanto { get; set; }

    /// <summary>
    /// Almacena el impuesto de ventas de la factura
    /// </summary>
    public decimal ImpuestoVentas { get; set; }

    /// <summary>
    /// Estado de la factura
    /// </summary>
    public int EstadoFacturaId { get; set; }

    public virtual EstadoFactura EstadoFactura { get; set; } = null!;

    public virtual OrdenServicio? OrdenServicio { get; set; }

    public virtual ICollection<Reparacion> Reparaciones { get; set; } = new List<Reparacion>();

    public virtual ICollection<Repuesto> Repuestos { get; set; } = new List<Repuesto>();

    public virtual Vehiculo Vehiculo { get; set; } = null!;
}

