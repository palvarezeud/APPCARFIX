using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena el catalogo con los costos vigentes de reparaciones comunes
/// </summary>
public partial class TipoReparacion
{
    /// <summary>
    /// Identificador único de la tabla
    /// </summary>
    public int TipoReparacionId { get; set; }

    /// <summary>
    /// Describe la reparación a realizar
    /// </summary>
    public string DescripcionReparacion { get; set; } = null!;

    /// <summary>
    /// Detalla cuánto aproximadamente se tarda en horas
    /// </summary>
    public int DuracionAproximadaHoras { get; set; }

    /// <summary>
    /// Muestra un costo base 
    /// </summary>
    public decimal CostoBase { get; set; }

}

