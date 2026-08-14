import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorApiInterceptor } from './core/interceptors/error-api.interceptor';
import { ESTUDIANTE_REPOSITORY } from './domain/repositories/estudiante.repository';
import { EstudianteHttpRepository } from './infrastructure/http/estudiante-http.repository';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([jwtInterceptor, errorApiInterceptor])),

    // Unico lugar donde el puerto del dominio se ata a su adaptador HTTP.
    { provide: ESTUDIANTE_REPOSITORY, useExisting: EstudianteHttpRepository },
  ],
};
