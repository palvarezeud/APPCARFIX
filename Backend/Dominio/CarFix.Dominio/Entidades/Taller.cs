using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Datos del taller
/// </summary>
public partial class Taller
{
    /// <summary>
    /// Identificador unico
    /// </summary>
    public int TallerId { get; set; }

    /// <summary>
    /// Nombre del taller
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Descripción de la ubicación
    /// </summary>
    public string UbicacionDescripcion { get; set; } = null!;

    /// <summary>
    /// Telefonos del taller
    /// </summary>
    public string Telefonos { get; set; } = null!;

    /// <summary>
    /// Correo electronico del taller
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Coordenadas geograficas del taller (columna BD: UbicaciónGPS, con tilde — pendiente renombrar)
    /// </summary>
    public Point? UbicacionGps { get; set; }
}

