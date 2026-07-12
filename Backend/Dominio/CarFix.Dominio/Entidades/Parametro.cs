using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Catalogo generico clave/valor para parametros de configuracion del sistema
/// </summary>
public partial class Parametro
{
    /// <summary>
    /// Identificador unico del parametro
    /// </summary>
    public int ParametroId { get; set; }

    /// <summary>
    /// Nombre/clave del parametro
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Valor del parametro (texto libre)
    /// </summary>
    public string Valor { get; set; } = null!;
}
