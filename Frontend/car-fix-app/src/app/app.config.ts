import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideAppInitializer, inject, isDevMode } from '@angular/core';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { provideServiceWorker } from '@angular/service-worker';
import { SaludService } from './core/comun/salud.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
    // Dispara la BD (Azure SQL Serverless puede estar pausada) sin bloquear el arranque de la app
    provideAppInitializer(() => {
      inject(SaludService).activar().subscribe({ error: () => {} });
    }),
  ],
};
