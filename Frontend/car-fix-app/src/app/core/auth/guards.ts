import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const autenticadoGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() || inject(Router).createUrlTree(['/login']);
};

export const soloAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.esAdmin() || inject(Router).createUrlTree(['/sin-acceso']);
};

export const soloJefeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.esJefe() || inject(Router).createUrlTree(['/sin-acceso']);
};
