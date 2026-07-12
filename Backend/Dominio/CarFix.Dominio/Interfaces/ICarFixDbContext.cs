using CarFix.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CarFix.Dominio.Interfaces;

public interface ICarFixDbContext
{
    DbSet<Cliente>             Clientes             { get; }
    DbSet<Vehiculo>            Vehiculos            { get; }
    DbSet<OrdenServicio>       OrdenServicios       { get; }
    DbSet<Factura>             Facturas             { get; }
    DbSet<Reparacion>          Reparacions          { get; }
    DbSet<Repuesto>            Repuestos            { get; }
    DbSet<TipoReparacion>      TipoReparacions      { get; }
    DbSet<HistoricoRespuesto>  HistoricoRespuestos  { get; }
    DbSet<EstadoOrden>         EstadoOrdens         { get; }
    DbSet<EstadoFactura>       EstadoFacturas       { get; }
    DbSet<Role>                Roles                { get; }
    DbSet<Usuario>             Usuarios             { get; }
    DbSet<Taller>              Tallers              { get; }
    DbSet<MarcaModelo>         MarcaModelos         { get; }
    DbSet<Parametro>           Parametros           { get; }

    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
