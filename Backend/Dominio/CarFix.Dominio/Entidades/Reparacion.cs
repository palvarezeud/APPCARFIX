using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena las reparaciones hechas a un vehículo
/// </summary>
public partial class Reparacion
{
    /// <summary>
    /// Identificador unico de la reparación
    /// </summary>
    public int ReparacionId { get; set; }

    /// <summary>
    /// Identificador de la orden asociada a la reparación
    /// </summary>
    public int FacturaId { get; set; }

    public bool Listo { get; set; }

    /// <summary>
    /// Descripción de la reparación hecha
    /// </summary>
    public string DescripcionReparacion { get; set; } = null!;

    /// <summary>
    /// Duracion aproximada de la reparacion en horas
    /// </summary>
    public int? DuracionAproximadaHoras { get; set; }

    /// <summary>
    /// Costo de la reparación
    /// </summary>
    public decimal Costo { get; set; }

    public virtual Factura Factura { get; set; } = null!;
}

