using CarFix.Dominio.Entidades;
using CarFix.Infraestructura.Persistencia;
using Reqnroll;

namespace CarFix.Especificaciones.Soporte;

[Binding]
public class LimpiezaBase
{
    private readonly CarFixDbContext _contexto;

    public LimpiezaBase(CarFixDbContext contexto) => _contexto = contexto;

    [BeforeScenario(Order = 0)]
    public void AntesDeCadaEscenario()
    {
        _contexto.Database.EnsureDeleted();

        _contexto.EstadoOrdens.AddRange(
            new EstadoOrden { EstadoOrdenId = 1, Descripcion = "Cotizacion"    },
            new EstadoOrden { EstadoOrdenId = 2, Descripcion = "Recibido"      },
            new EstadoOrden { EstadoOrdenId = 3, Descripcion = "En reparacion" },
            new EstadoOrden { EstadoOrdenId = 4, Descripcion = "Finalizado"    },
            new EstadoOrden { EstadoOrdenId = 5, Descripcion = "Entregado"     }
        );

        _contexto.EstadoFacturas.AddRange(
            new EstadoFactura { EstadoFacturaId = 1, Descipcion = "Cotizacion" },
            new EstadoFactura { EstadoFacturaId = 2, Descipcion = "Pendiente"  },
            new EstadoFactura { EstadoFacturaId = 3, Descipcion = "Pagada"     }
        );

        _contexto.Roles.AddRange(
            new Role { RolId = 1, Nombre = "Administrador" },
            new Role { RolId = 2, Nombre = "JefeMecanicos" },
            new Role { RolId = 3, Nombre = "Mecanico"      }
        );

        _contexto.SaveChanges();
    }
}
