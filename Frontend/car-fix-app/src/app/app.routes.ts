import { Routes } from '@angular/router';
import { autenticadoGuard, soloAdminGuard, soloJefeGuard } from './core/auth/guards';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [autenticadoGuard],
    loadComponent: () => import('./features/shell/shell.component').then(m => m.ShellComponent),
    children: [
      { path: 'chat',        loadComponent: () => import('./features/chat-taller/chat-taller.component').then(m => m.ChatTallerComponent) },
      { path: 'clientes',    loadComponent: () => import('./features/clientes/clientes.component').then(m => m.ClientesComponent) },
      { path: 'vehiculos',   loadComponent: () => import('./features/vehiculos/vehiculos.component').then(m => m.VehiculosComponent) },
      { path: 'ordenes',     loadComponent: () => import('./features/ordenes/ordenes.component').then(m => m.OrdenesComponent) },
      { path: 'facturas',    loadComponent: () => import('./features/facturas/facturas.component').then(m => m.FacturasComponent) },
      {
        path: 'catalogo-reparaciones',
        canActivate: [soloJefeGuard],
        loadComponent: () => import('./features/catalogo-reparaciones/catalogo-reparaciones.component').then(m => m.CatalogoReparacionesComponent)
      },
      {
        path: 'catalogo-repuestos',
        canActivate: [soloJefeGuard],
        loadComponent: () => import('./features/catalogo-repuestos/catalogo-repuestos.component').then(m => m.CatalogoRepuestosComponent)
      },
      {
        path: 'usuarios',
        canActivate: [soloJefeGuard],
        loadComponent: () => import('./features/usuarios/usuarios.component').then(m => m.UsuariosComponent)
      },
      {
        path: 'configuracion',
        canActivate: [soloAdminGuard],
        loadComponent: () => import('./features/configuracion/configuracion.component').then(m => m.ConfiguracionComponent)
      },
      { path: '', loadComponent: () => import('./features/inicio/inicio.component').then(m => m.InicioComponent) }
    ]
  },
  {
    path: 'sin-acceso',
    loadComponent: () => import('./features/sin-acceso/sin-acceso.component').then(m => m.SinAccesoComponent)
  },
  { path: '**', redirectTo: '' }
];
