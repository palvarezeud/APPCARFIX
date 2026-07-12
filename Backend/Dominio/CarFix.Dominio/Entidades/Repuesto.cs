using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena los repuestos que se cotizan o aplican a la reparación
/// </summary>
public partial class Repuesto
{
    /// <summary>
    /// Identificador único del respuesto
    /// </summary>
    public int RepuestoId { get; set; }

    /// <summary>
    /// Identificador de la orden asociada
    /// </summary>
    public int FacturaId { get; set; }

    /// <summary>
    /// Descripción del respuesto
    /// </summary>
    public string NombreRepuesto { get; set; } = null!;

    /// <summary>
    /// Costo del repuesto
    /// </summary>
    public decimal Costo { get; set; }

    /// <summary>
    /// Fecha de compra del repuesto
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Nombre del lugar donde se compró el repuesto
    /// </summary>
    public string Repuestera { get; set; } = null!;

    /// <summary>
    /// Número de factura asociada
    /// </summary>
    public string? Factura { get; set; }

    /// <summary>
    /// Indica si el mecanico ya incluyo/instalo este repuesto en el vehiculo
    /// </summary>
    public bool Incluido { get; set; }

    public virtual Factura FacturaNavigation { get; set; } = null!;
}

