using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena los estados de las facturas(Cotizacion, Pendiente, Cancelada)
/// </summary>
public partial class EstadoFactura
{
    /// <summary>
    /// Identificador único del estado de la factura
    /// </summary>
    public int EstadoFacturaId { get; set; }

    /// <summary>
    /// Almacena la descripción del estado
    /// </summary>
    public string Descipcion { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}

