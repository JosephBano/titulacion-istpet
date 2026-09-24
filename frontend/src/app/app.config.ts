import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_BASE_HREF } from '@angular/common';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withComponentInputBinding, withHashLocation } from '@angular/router';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorApiInterceptor } from './core/interceptors/error-api.interceptor';
import { ESTUDIANTE_REPOSITORY } from './domain/repositories/estudiante.repository';
import { EstudianteHttpRepository } from './infrastructure/http/estudiante-http.repository';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: APP_BASE_HREF, useValue: '/appTitulacion/' },
    provideRouter(routes, withComponentInputBinding(), withHashLocation()),
    provideHttpClient(withInterceptors([jwtInterceptor, errorApiInterceptor])),
    provideAnimationsAsync(),

    // Unico lugar donde el puerto del dominio se ata a su adaptador HTTP.
    { provide: ESTUDIANTE_REPOSITORY, useExisting: EstudianteHttpRepository },
  ],
};
