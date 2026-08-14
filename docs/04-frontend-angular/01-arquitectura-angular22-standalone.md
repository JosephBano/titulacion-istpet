# Arquitectura Frontend Angular 22 Standalone y Signals — Sistema Titulación ISTPET

## 1. Principios de Arquitectura Frontend

El cliente web de Titulación ISTPET está desarrollado en **Angular 22** aprovechando las características modernas del framework.

### Características Clave:
1. **Componentes Standalone:** Eliminación completa de `NgModule`. Todos los componentes, directivas y pipes se importan explícitamente a nivel de componente.
2. **Estado Reactivo con Signals:** Uso prioritario de **Angular Signals** (`signal`, `computed`, `effect`) para la reactividad de la interfaz gráfica y estado de UI, reduciendo la necesidad de suscripciones manuales a observables RxJS.
3. **RxJS para Flujos Asíncronos HTTP:** Combinación de RxJS (`Observable`, `HttpClient`, `pipe`, `catchError`) para la comunicación con la API REST backend.
4. **Carga Diferida (Lazy Loading):** Enrutamiento estructurado mediante `loadComponent` en `app.routes.ts` para optimizar el bundle inicial del navegador.

---

## 2. Estructura de Directorios del Proyecto Frontend

```
frontend/src/
├── app/
│   ├── core/                   <- Servicios globales, guardias, interceptores y modelos
│   │   ├── guards/             <- auth.guard.ts, permission.guard.ts
│   │   ├── interceptors/       <- auth.interceptor.ts, error.interceptor.ts
│   │   ├── models/             <- Interfaces TypeScript (User, AuthResponse, Permission)
│   │   └── services/           <- auth.service.ts, rbac.service.ts, academico.service.ts
│   │
│   ├── pages/                  <- Vistas principales de la aplicación
│   │   └── login/              <- Componente de inicio de sesión
│   │
│   ├── app.component.ts        <- Componente raíz Standalone
│   ├── app.config.ts           <- Configuración de proveedores (provideRouter, provideHttpClient)
│   └── app.routes.ts           <- Definición del árbol de rutas de la aplicación
│
├── assets/                     <- Recursos estáticos (Logotipo ISTPET, favicon)
├── styles.css                  <- Estilos globales y tokens de diseño Fluent 2
└── index.html
```

---

## 3. Configuración Principal de la Aplicación (`app.config.ts`)

La configuración global de Angular 22 se realiza sin `AppModule`, utilizando proveedores funcionales:

```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    )
  ]
};
```


