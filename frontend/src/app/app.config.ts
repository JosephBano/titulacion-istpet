import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_BASE_HREF } from '@angular/common';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withComponentInputBinding, withHashLocation } from '@angular/router';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorApiInterceptor } from './core/interceptors/error-api.interceptor';
import { ESTUDIANTE_REPOSITORY } from './domain/repositories/estudiante.repository';
import { EstudianteHttpRepository } from './infrastructure/http/estudiante-http.repository';
import { COMPLEXIVO_REPOSITORY } from './domain/repositories/complexivo.repository';
import { ComplexivoHttpRepository } from './infrastructure/http/complexivo-http.repository';
import { PLANTILLA_REQUISICION_REPOSITORY } from './domain/repositories/plantilla-requisicion.repository';
import { PlantillaRequisicionHttpRepository } from './infrastructure/http/plantilla-requisicion-http.repository';
import { routes } from './app.routes';

import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: APP_BASE_HREF, useValue: environment.produccion ? '/appTitulacion/' : '/' },
    provideRouter(routes, withComponentInputBinding(), withHashLocation()),
    provideHttpClient(withInterceptors([jwtInterceptor, errorApiInterceptor])),
    provideAnimationsAsync(),

    // Unico lugar donde el puerto del dominio se ata a su adaptador HTTP.
    { provide: ESTUDIANTE_REPOSITORY, useExisting: EstudianteHttpRepository },
    { provide: COMPLEXIVO_REPOSITORY, useExisting: ComplexivoHttpRepository },
    { provide: PLANTILLA_REQUISICION_REPOSITORY, useExisting: PlantillaRequisicionHttpRepository },
  ],
};
