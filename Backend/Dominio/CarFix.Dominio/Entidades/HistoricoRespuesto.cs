using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena un historico con repuestos comprados en facturas anteriores
/// </summary>
public partial class HistoricoRespuesto
{
    /// <summary>
    /// Identificador único de la tabla
    /// </summary>
    public int RespuestoHistoricoId { get; set; }

    /// <summary>
    /// Marca del Vehículo
    /// </summary>
    public string Marca { get; set; } = null!;

    /// <summary>
    /// Modelo del vehículo
    /// </summary>
    public string Modelo { get; set; } = null!;

    /// <summary>
    /// Año del vehículo
    /// </summary>
    public int Annio { get; set; }

    /// <summary>
    /// Descripción del repuesto
    /// </summary>
    public string RepuestoDecripcion { get; set; } = null!;

    /// <summary>
    /// Precio del repuesto
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Lugar donde se compró
    /// </summary>
    public string Repuestera { get; set; } = null!;

    /// <summary>
    /// Fecha que se compro
    /// </summary>
    public DateTime FechaCompra { get; set; }
}

