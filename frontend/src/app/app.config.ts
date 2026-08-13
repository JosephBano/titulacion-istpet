import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorApiInterceptor } from './core/interceptors/error-api.interceptor';
import { ESTUDIANTE_REPOSITORY } from './domain/repositories/estudiante.repository';
import { EstudianteHttpRepository } from './infrastructure/http/estudiante-http.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([jwtInterceptor, errorApiInterceptor])),
    { provide: ESTUDIANTE_REPOSITORY, useExisting: EstudianteHttpRepository },
  ],
};
