using System;
using System.Collections.Generic;

namespace CarFix.Dominio.Entidades;

/// <summary>
/// Almacena los estados de las ordenes de reparación(Cotización, Recibido, En reparacion, Finalizado, Entregado)
/// </summary>
public partial class EstadoOrden
{
    /// <summary>
    /// Identificador del estado de la orden
    /// </summary>
    public int EstadoOrdenId { get; set; }

    /// <summary>
    /// Descripción del estado de una orden
    /// </summary>
    public string Descripcion { get; set; } = null!;

    public virtual ICollection<OrdenServicio> OrdenServicios { get; set; } = new List<OrdenServicio>();
}

